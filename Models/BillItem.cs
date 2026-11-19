using System;

namespace SupermarketManagementSystem
{
    public class BillItem
    {
        public int BillItemId { get; private set; }
        public int BillId { get; private set; }
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal => Quantity * UnitPrice;

        public BillItem(string productId, string productName, int quantity, decimal unitPrice,
                        int billItemId = 0, int billId = 0)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("Thiếu mã sản phẩm.", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(quantity));
            if (unitPrice < 0) throw new ArgumentException("Đơn giá không được âm.", nameof(unitPrice));
            ProductId = productId;
            ProductName = productName ?? string.Empty;
            Quantity = quantity;
            UnitPrice = unitPrice;
            BillItemId = billItemId;
            BillId = billId;
        }

        public static BillItem FromCartItem(CartItem cart)
            => new BillItem(cart.Product.ProductId, cart.Product.Name, cart.SelectedQuantity, cart.Product.SellingPrice);

        internal void IncreaseQuantity(int amount)
        {
            if (amount <= 0) throw new ArgumentException("Số lượng thêm phải lớn hơn 0.", nameof(amount));
            Quantity += amount;
        }

        internal void AssignBill(int billId) => BillId = billId;

        public void AssignId(int billItemId)
        {
            if (BillItemId != 0) throw new InvalidOperationException("Mã dòng chi tiết đã được gán.");
            BillItemId = billItemId;
        }
    }
}
