using System;

namespace SupermarketManagementSystem
{
    public class Product
    {
        public const int DefaultLowStockThreshold = 20; //Ngường tồn kho tối thiểu là 20

        public string ProductId { get; private set; }
        public string Barcode { get; private set; }
        public string Name { get; private set; }
        public decimal ImportPrice { get; private set; }
        public decimal SellingPrice { get; private set; }
        public int StockQuantity { get; private set; }
        public string Unit { get; private set; }
        public string CategoryId { get; private set; }
        public string SupplierId { get; private set; }
        public bool IsActive { get; private set; }

        public Product(string productId, string barcode, string name, decimal importPrice, decimal sellingPrice,
                       int stockQuantity, string unit, string categoryId, string supplierId, bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("Mã sản phẩm trống.", nameof(productId));
            if (stockQuantity < 0) throw new ArgumentException("Tồn kho không được âm.", nameof(stockQuantity));
            if (string.IsNullOrWhiteSpace(categoryId)) throw new ArgumentException("Thiếu danh mục.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(supplierId)) throw new ArgumentException("Thiếu nhà cung cấp.", nameof(supplierId));

            ProductId = productId.Trim();
            CategoryId = categoryId.Trim();
            SupplierId = supplierId.Trim();
            StockQuantity = stockQuantity;
            IsActive = isActive;
            UpdateInfo(barcode, name, unit);
            UpdatePrice(importPrice, sellingPrice);
        }

        public void UpdateInfo(string barcode, string name, string unit)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên sản phẩm trống.", nameof(name));
            if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Đơn vị tính trống.", nameof(unit));
            Barcode = barcode?.Trim() ?? string.Empty;
            Name = name.Trim();
            Unit = unit.Trim();
        }

        // Cập nhật thông tin, giá và danh mục trong một lần: kiểm tra hết trước khi gán
        // để lỗi ở bước sau không làm đối tượng bị sửa dở dang.
        public void UpdateDetails(string barcode, string name, string unit,
                                decimal importPrice, decimal sellingPrice, string categoryId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên sản phẩm trống.", nameof(name));
            if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Đơn vị tính trống.", nameof(unit));
            if (string.IsNullOrWhiteSpace(categoryId)) throw new ArgumentException("Thiếu danh mục.", nameof(categoryId));
            if (importPrice < 0 || sellingPrice < 0) throw new ArgumentException("Giá không được âm.");
            if (sellingPrice < importPrice) throw new ArgumentException("Giá bán phải lớn hơn hoặc bằng giá nhập.");

            UpdateInfo(barcode, name, unit);
            UpdatePrice(importPrice, sellingPrice);
            ChangeCategory(categoryId);
        }
        
        public void ChangeCategory(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId)) throw new ArgumentException("Thiếu danh mục.", nameof(categoryId));
            CategoryId = categoryId.Trim();
        }

        //BR06: giá bán >= giá nhập và >= 0.
        public void UpdatePrice(decimal importPrice, decimal sellingPrice)
        {
            if (importPrice < 0 || sellingPrice < 0)
                throw new ArgumentException("Giá không được âm.");
            if (sellingPrice < importPrice)
                throw new ArgumentException("Giá bán phải lớn hơn hoặc bằng giá nhập.");
            ImportPrice = importPrice;
            SellingPrice = sellingPrice;
        }

        //BR05: sản phẩm đang bán và đủ tồn kho.
        public bool CanSell(int requestedQuantity)
            => IsActive && requestedQuantity > 0 && StockQuantity >= requestedQuantity;

        //BR05: trừ tồn khi thanh toán thành công.
        public void DecreaseStock(int amount)
        {
            if (!CanSell(amount))
                throw new InvalidOperationException($"Sản phẩm '{Name}' không đủ tồn kho hoặc đã ngừng kinh doanh.");
            StockQuantity -= amount;
        }

        //BR06: nhập kho, số lượng phải > 0.
        public void IncreaseStock(int amount)
        {
            if (amount <= 0) throw new ArgumentException("Số lượng nhập phải lớn hơn 0.", nameof(amount));
            StockQuantity += amount;
        }

        //UC06: điều chỉnh tồn (hủy, hao hụt, kiểm kê). Không cho tồn âm.
        public void AdjustStock(int delta)
        {
            if (delta == 0) throw new ArgumentException("Lượng điều chỉnh phải khác 0.", nameof(delta));
            if (StockQuantity + delta < 0) throw new InvalidOperationException("Điều chỉnh làm tồn kho bị âm.");
            StockQuantity += delta;
        }

        //BR07: sản phẩm đang kinh doanh và tồn <= ngưỡng.
        public bool IsLowStock(int threshold = DefaultLowStockThreshold)
            => IsActive && StockQuantity <= threshold;

        public void Disable() => IsActive = false;
        public void Enable() => IsActive = true;

        public override string ToString() => $"{ProductId} - {Name} ({StockQuantity} {Unit})";
    }
}
