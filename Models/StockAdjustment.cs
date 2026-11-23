using System;

namespace SupermarketManagementSystem
{
    public class StockAdjustment
    {
        public string AdjustmentId { get; private set; }
        public string ProductId { get; private set; }
        public string EmployeeId { get; private set; }
        public int QuantityChange { get; private set; }
        public AdjustmentReason Reason { get; private set; }
        public string Note { get; private set; }
        public DateTime AdjustedAt { get; private set; }

        public StockAdjustment(
            string adjustmentId,
            string productId,
            string employeeId,
            int quantityChange,
            AdjustmentReason reason,
            string note,
            DateTime? adjustedAt = null)
        {
            if (string.IsNullOrWhiteSpace(adjustmentId))
                throw new ArgumentException("Mã phiếu điều chỉnh trống.", nameof(adjustmentId));
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Thiếu mã sản phẩm.", nameof(productId));
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Thiếu mã nhân viên thực hiện.", nameof(employeeId));

            AdjustmentId = adjustmentId.Trim();
            ProductId = productId.Trim();
            EmployeeId = employeeId.Trim();
            QuantityChange = quantityChange;
            Reason = reason;
            Note = note?.Trim() ?? string.Empty;
            AdjustedAt = adjustedAt ?? DateTime.Now;

            Validate();
        }

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(AdjustmentReason), Reason))
                throw new ArgumentException("Lý do điều chỉnh không hợp lệ.");

            if (string.IsNullOrWhiteSpace(Note))
                throw new ArgumentException("Bắt buộc nhập ghi chú (Note) khi điều chỉnh tồn kho.");

            if (QuantityChange == 0)
                throw new ArgumentException("Mức thay đổi tồn kho (QuantityChange) phải khác 0.");

            bool mustDecrease = Reason == AdjustmentReason.Damaged
                             || Reason == AdjustmentReason.Expired
                             || Reason == AdjustmentReason.Lost;

            if (mustDecrease && QuantityChange > 0)
                throw new ArgumentException($"Lý do {Reason} bắt buộc làm giảm tồn kho (QuantityChange < 0).");
        }

        public void ApplyTo(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            if (!string.Equals(product.ProductId, ProductId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Phiếu điều chỉnh không thuộc sản phẩm này.");

            product.AdjustStock(QuantityChange);
        }
    }
}