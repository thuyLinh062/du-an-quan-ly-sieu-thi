using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketManagementSystem
{
    public class ProductService : BaseService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISupplierRepository _supplierRepository;

        public ProductService(
            ISessionContext session,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ISupplierRepository supplierRepository)
            : base(session)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
        }

        public void AddProduct(
            string productId,
            string barcode,
            string name,
            decimal importPrice,
            decimal sellingPrice,
            int stockQuantity,
            string unit,
            string categoryId,
            string supplierId,
            bool isActive = true)
        {
            RequireManagement();

            string cleanBarcode = barcode?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(cleanBarcode))
            {
                var existingByBarcode = _productRepository.GetByBarcode(cleanBarcode);
                if (existingByBarcode != null)
                    throw new BusinessRuleException($"Barcode '{cleanBarcode}' đã tồn tại ở sản phẩm khác.");
            }

            string cleanCategoryId = categoryId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cleanCategoryId) || _categoryRepository.GetById(cleanCategoryId) == null)
                throw new BusinessRuleException($"Danh mục '{cleanCategoryId}' không tồn tại.");

            string cleanSupplierId = supplierId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cleanSupplierId) || _supplierRepository.GetById(cleanSupplierId) == null)
                throw new BusinessRuleException($"Nhà cung cấp '{cleanSupplierId}' không tồn tại.");

            try
            {
                var product = new Product(
                    productId,
                    cleanBarcode,
                    name,
                    importPrice,
                    sellingPrice,
                    stockQuantity,
                    unit,
                    cleanCategoryId,
                    cleanSupplierId,
                    isActive);

                _productRepository.Add(product);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public void UpdateProduct(
            string productId,
            string barcode,
            string name,
            string unit,
            decimal importPrice,
            decimal sellingPrice,
            string categoryId)
        {
            RequireManagement();

            string cleanId = productId?.Trim() ?? string.Empty;
            var product = _productRepository.GetById(cleanId);
            if (product == null)
                throw new BusinessRuleException($"Không tìm thấy sản phẩm có mã '{cleanId}'.");

            string cleanBarcode = barcode?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(cleanBarcode))
            {
                var existingByBarcode = _productRepository.GetByBarcode(cleanBarcode);
                if (existingByBarcode != null &&
                    !string.Equals(existingByBarcode.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessRuleException($"Barcode '{cleanBarcode}' đã được sử dụng bởi sản phẩm khác.");
                }
            }

            string cleanCategoryId = categoryId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cleanCategoryId) || _categoryRepository.GetById(cleanCategoryId) == null)
                throw new BusinessRuleException($"Danh mục '{cleanCategoryId}' không tồn tại.");

            try
            {
                product.UpdateInfo(cleanBarcode, name, unit);
                product.UpdatePrice(importPrice, sellingPrice);
                product.ChangeCategory(cleanCategoryId);
                _productRepository.Update(product);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public void DisableProduct(string productId)
        {
            RequireManagement();

            string cleanId = productId?.Trim() ?? string.Empty;
            var product = _productRepository.GetById(cleanId);
            if (product == null)
                throw new BusinessRuleException($"Không tìm thấy sản phẩm có mã '{cleanId}'.");

            product.Disable();
            _productRepository.Update(product);
        }

        public void EnableProduct(string productId)
        {
            RequireManagement();

            string cleanId = productId?.Trim() ?? string.Empty;
            var product = _productRepository.GetById(cleanId);
            if (product == null)
                throw new BusinessRuleException($"Không tìm thấy sản phẩm có mã '{cleanId}'.");

            product.Enable();
            _productRepository.Update(product);
        }

        public List<Product> SearchProducts(string? keyword = null, string? categoryId = null, bool? onlyActive = null)
        {
            RequireAuthenticatedRead();

            IEnumerable<Product> query = _productRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    p.ProductId.ToLowerInvariant().Contains(kw) ||
                    p.Barcode.ToLowerInvariant().Contains(kw) ||
                    p.Name.ToLowerInvariant().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                string catId = categoryId.Trim();
                query = query.Where(p => string.Equals(p.CategoryId, catId, StringComparison.OrdinalIgnoreCase));
            }

            if (onlyActive.HasValue)
            {
                query = query.Where(p => p.IsActive == onlyActive.Value);
            }

            return query.ToList();
        }

        public List<Category> GetCategories(bool onlyActive = false)
        {
            RequireAuthenticatedRead();

            var categories = _categoryRepository.GetAll();
            return onlyActive
                ? categories.Where(c => c.IsActive).ToList()
                : categories;
        }

        public void AddCategory(string categoryId, string name, string description = "", bool isActive = true)
        {
            RequireManagement();

            string cleanId = categoryId?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(cleanId) && _categoryRepository.GetById(cleanId) != null)
                throw new BusinessRuleException($"Mã danh mục '{cleanId}' đã tồn tại.");

            try
            {
                var category = new Category(cleanId, name, description, isActive);
                _categoryRepository.Add(category);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public void UpdateCategory(string categoryId, string name, string description = "")
        {
            RequireManagement();

            string cleanId = categoryId?.Trim() ?? string.Empty;
            var category = _categoryRepository.GetById(cleanId);
            if (category == null)
                throw new BusinessRuleException($"Không tìm thấy danh mục '{cleanId}'.");

            try
            {
                category.Update(name, description);
                _categoryRepository.Update(category);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public void SetCategoryActive(string categoryId, bool isActive)
        {
            RequireManagement();

            string cleanId = categoryId?.Trim() ?? string.Empty;
            var category = _categoryRepository.GetById(cleanId);
            if (category == null)
                throw new BusinessRuleException($"Không tìm thấy danh mục '{cleanId}'.");

            category.SetActive(isActive);
            _categoryRepository.Update(category);
        }
    }
}