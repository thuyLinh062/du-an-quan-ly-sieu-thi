using System;

namespace SupermarketManagementSystem
{
    public class ProductSales
    {
        public string ProductId { get; }
        public string ProductName { get; }
        public int QuantitySold { get; }
        public decimal Revenue { get; }

        public ProductSales(string productId, string productName, int quantitySold, decimal revenue)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Mã sản phẩm trống.", nameof(productId));
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Tên sản phẩm trống.", nameof(productName));
            if (quantitySold < 0 || revenue < 0)
                throw new ArgumentException("Số liệu thống kê sản phẩm không được âm.");

            ProductId = productId.Trim();
            ProductName = productName.Trim();
            QuantitySold = quantitySold;
            Revenue = revenue;
        }
    }
}