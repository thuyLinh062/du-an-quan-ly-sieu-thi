using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketManagementSystem
{
    public class CategoryRevenue
    {
        public string CategoryId { get; private set; }
        public string CategoryName { get; private set; }
        public decimal Revenue { get; private set; }

        public CategoryRevenue(string categoryId, string categoryName, decimal revenue)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Mã danh mục trống.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentException("Tên danh mục trống.", nameof(categoryName));
            if (revenue < 0)
                throw new ArgumentException("Doanh thu danh mục không được âm.", nameof(revenue));

            CategoryId = categoryId.Trim();
            CategoryName = categoryName.Trim();
            Revenue = revenue;
        }
    }

    public class ReportService : BaseService
    {
        private readonly IBillRepository _billRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ReportService(
            ISessionContext session,
            IBillRepository billRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
            : base(session)
        {
            _billRepository = billRepository ?? throw new ArgumentNullException(nameof(billRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public List<RevenueReport> GetRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            RequireManagement();
            ValidateDateRange(startDate, endDate);

            DateTime from = startDate.Date;
            DateTime toExclusive = endDate.Date.AddDays(1);

            var validBills = _billRepository.GetByDateRange(from, toExclusive)
                .Where(b => b.CreatedAt >= from && b.CreatedAt < toExclusive)
                .ToList();

            if (validBills.Count == 0)
                return new List<RevenueReport>();

            return validBills
                .GroupBy(b => b.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueReport(
                    reportDate: g.Key,
                    totalBills: g.Count(),
                    totalRevenue: g.Sum(b => b.FinalAmount),
                    totalPointsUsed: g.Sum(b => b.AppliedPoints),
                    uniqueCustomers: g.Where(b => !string.IsNullOrWhiteSpace(b.CustomerId))
                                      .Select(b => b.CustomerId!.Trim())
                                      .Distinct()
                                      .Count()))
                .ToList();
        }

        public List<ProductSales> GetTopSellingProducts(DateTime startDate, DateTime endDate, int topN = 10)
        {
            RequireManagement();
            ValidateDateRange(startDate, endDate);

            if (topN <= 0)
                throw new BusinessRuleException("Số lượng top sản phẩm phải lớn hơn 0.");

            DateTime from = startDate.Date;
            DateTime toExclusive = endDate.Date.AddDays(1);

            var validBills = _billRepository.GetByDateRange(from, toExclusive)
                .Where(b => b.CreatedAt >= from && b.CreatedAt < toExclusive)
                .ToList();

            var items = validBills.SelectMany(b => b.Items).ToList();
            if (items.Count == 0)
                return new List<ProductSales>();

            var productsById = _productRepository.GetAll()
                .ToDictionary(p => p.ProductId, p => p.Name, StringComparer.OrdinalIgnoreCase);

            return items
                .GroupBy(i => i.ProductId)
                .Select(g =>
                {
                    string prodName = productsById.TryGetValue(g.Key, out var name)
                        ? name
                        : g.First().ProductName;

                    return new ProductSales(
                        productId: g.Key,
                        productName: prodName,
                        quantitySold: g.Sum(i => i.Quantity),
                        revenue: g.Sum(i => i.LineTotal));
                })
                .OrderByDescending(ps => ps.QuantitySold)
                .ThenByDescending(ps => ps.Revenue)
                .Take(topN)
                .ToList();
        }

        public List<CategoryRevenue> GetRevenueByCategory(DateTime startDate, DateTime endDate)
        {
            RequireManagement();
            ValidateDateRange(startDate, endDate);

            DateTime from = startDate.Date;
            DateTime toExclusive = endDate.Date.AddDays(1);

            var validBills = _billRepository.GetByDateRange(from, toExclusive)
                .Where(b => b.CreatedAt >= from && b.CreatedAt < toExclusive)
                .ToList();

            var items = validBills.SelectMany(b => b.Items).ToList();
            if (items.Count == 0)
                return new List<CategoryRevenue>();

            var products = _productRepository.GetAll()
                .ToDictionary(p => p.ProductId, p => p, StringComparer.OrdinalIgnoreCase);

            var categories = _categoryRepository.GetAll()
                .ToDictionary(c => c.CategoryId, c => c.Name, StringComparer.OrdinalIgnoreCase);

            return items
                .GroupBy(i =>
                {
                    if (products.TryGetValue(i.ProductId, out var prod) &&
                        !string.IsNullOrWhiteSpace(prod.CategoryId))
                    {
                        return prod.CategoryId;
                    }
                    return "UNKNOWN";
                })
                .Select(g =>
                {
                    string catName = categories.TryGetValue(g.Key, out var name)
                        ? name
                        : "Không xác định";

                    return new CategoryRevenue(
                        categoryId: g.Key,
                        categoryName: catName,
                        revenue: g.Sum(i => i.LineTotal));
                })
                .OrderByDescending(cr => cr.Revenue)
                .ToList();
        }

        public List<Product> GetLowStockProducts(int threshold = Product.DefaultLowStockThreshold)
        {
            RequireManagement();

            if (threshold < 0)
                throw new BusinessRuleException("Ngưỡng cảnh báo tồn kho không được âm.");

            return _productRepository.GetAll()
                .Where(p => p.IsLowStock(threshold))
                .ToList();
        }

        private static void ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate.Date > endDate.Date)
                throw new BusinessRuleException("Ngày bắt đầu (StartDate) không được lớn hơn ngày kết thúc (EndDate).");
        }
    }
}