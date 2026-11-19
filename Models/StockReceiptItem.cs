using System;

namespace SupermarketManagementSystem
{
    public class StockReceiptItem
    {
        public int ReceiptItemId { get; private set; }
        public string ReceiptId { get; private set; }
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal ImportUnitPrice { get; private set; }
        public decimal LineTotal => Quantity * ImportUnitPrice;

        public StockReceiptItem(string receiptId, string productId, int quantity, decimal importUnitPrice, int receiptItemId = 0)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("Thiếu mã sản phẩm.", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Số lượng nhập phải lớn hơn 0.", nameof(quantity));
            if (importUnitPrice < 0) throw new ArgumentException("Đơn giá nhập không được âm.", nameof(importUnitPrice));
            ReceiptId = receiptId?.Trim() ?? string.Empty;
            ProductId = productId.Trim();
            Quantity = quantity;
            ImportUnitPrice = importUnitPrice;
            ReceiptItemId = receiptItemId;
        }

        public void AssignId(int id)
        {
            if(id <= 0) throw new ArgumentException("Mã dòng chi tiết phải lớn hơn 0.", nameof(id));
            if (ReceiptItemId != 0) throw new InvalidOperationException("Mã đã được gán.");
            ReceiptItemId = id;
        }
    }
}
