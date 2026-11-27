using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketManagementSystem
{
    public class StockReceipt
    {
        private readonly List<StockReceiptItem> _details = new List<StockReceiptItem>();

        public string ReceiptId { get; private set; }
        public string SupplierId { get; private set; }
        public string EmployeeId { get; private set; }
        public DateTime ImportDate { get; private set; }
        public string Note { get; private set; }
        public IReadOnlyList<StockReceiptItem> Details => _details.AsReadOnly();
        public decimal TotalCost => _details.Sum(d => d.LineTotal);

        public StockReceipt(string receiptId, string supplierId, string employeeId,
                            DateTime? importDate = null, string note = "")
        {
            if (string.IsNullOrWhiteSpace(receiptId)) throw new ArgumentException("Mã phiếu nhập trống.", nameof(receiptId));
            if (string.IsNullOrWhiteSpace(supplierId)) throw new ArgumentException("Thiếu nhà cung cấp.", nameof(supplierId));
            if (string.IsNullOrWhiteSpace(employeeId)) throw new ArgumentException("Thiếu nhân viên nhập.", nameof(employeeId));
            ReceiptId = receiptId.Trim();
            SupplierId = supplierId.Trim();
            EmployeeId = employeeId.Trim();
            ImportDate = importDate ?? DateTime.Now;
            Note = note?.Trim() ?? string.Empty;
        }

        public void AddItem(string productId, int quantity, decimal importUnitPrice, DateTime? expiryDate = null)
        {
            string cleanId = productId?.Trim() ?? string.Empty;
            var existing = _details.FirstOrDefault(d => string.Equals(d.ProductId, cleanId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                throw new InvalidOperationException($"Sản phẩm {cleanId} đã có trong phiếu nhập.");
            if (expiryDate.HasValue && expiryDate.Value.Date < ImportDate.Date)
            throw new ArgumentException("Hạn sử dụng không được trước ngày nhập.", nameof(expiryDate));
            _details.Add(new StockReceiptItem(ReceiptId, cleanId, quantity, importUnitPrice));
        }

        public void Validate()
        {
            if (_details.Count == 0) throw new InvalidOperationException("Phiếu nhập chưa có mặt hàng.");
        }
    }
}
