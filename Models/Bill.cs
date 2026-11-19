using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketManagementSystem
{
    public class Bill
    {
        private readonly List<BillItem> _items = new List<BillItem>();

        public int BillId { get; private set; }
        public string? CustomerId { get; private set; }          // null = khách vãng lai
        public string EmployeeId { get; private set; }
        public string? PromotionId { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public int AppliedPoints { get; private set; }
        public PaymentType PaymentMethod { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyList<BillItem> Items => _items.AsReadOnly();

        public decimal SubTotal => _items.Sum(i => i.LineTotal);
        public decimal PointDiscount => AppliedPoints * Customer.PointValue;
        public decimal FinalAmount => SubTotal - DiscountAmount - PointDiscount;

        public Bill(string employeeId, string? customerId = null, PaymentType paymentMethod = PaymentType.Cash,
                    DateTime? createdAt = null, int billId = 0)
        {
            if (string.IsNullOrWhiteSpace(employeeId)) throw new ArgumentException("Thiếu nhân viên lập hóa đơn.", nameof(employeeId));
            EmployeeId = employeeId;
            CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId;
            PaymentMethod = paymentMethod;
            CreatedAt = createdAt ?? DateTime.Now;
            BillId = billId;
        }

        public void AddItem(CartItem cart)
        {
            if (cart == null) throw new ArgumentNullException(nameof(cart));
            EnsureNoDiscountApplied();

            var existing = _items.FirstOrDefault(i => i.ProductId == cart.Product.ProductId);
            int totalQty = cart.SelectedQuantity + (existing?.Quantity ?? 0);
            if (!cart.Product.CanSell(totalQty))
                throw new InvalidOperationException($"Sản phẩm '{cart.Product.Name}' không đủ tồn kho.");

            if (existing == null) _items.Add(BillItem.FromCartItem(cart));
            else existing.IncreaseQuantity(cart.SelectedQuantity);
        }

        public void LoadItem(BillItem item) => _items.Add(item ?? throw new ArgumentNullException(nameof(item)));

        public void ApplyPromotion(Promotion promo, DateTime currentDate)
        {
            if (promo == null) throw new ArgumentNullException(nameof(promo));
            if (AppliedPoints > 0)
                throw new InvalidOperationException("Phải áp dụng khuyến mãi trước khi dùng điểm.");
            if (!promo.IsValid(SubTotal, currentDate))
                throw new InvalidOperationException("Khuyến mãi không hợp lệ hoặc hóa đơn chưa đạt giá trị tối thiểu.");
            PromotionId = promo.PromotionId;
            DiscountAmount = promo.CalculateDiscount(SubTotal);
        }

        public void RemovePromotion()
        {
            PromotionId = null;
            DiscountAmount = 0;
        }

        public void ApplyPoints(int points)
        {
            if (string.IsNullOrEmpty(CustomerId))
                throw new InvalidOperationException("Khách vãng lai không được dùng điểm.");
            if (points <= 0) throw new ArgumentException("Số điểm sử dụng phải lớn hơn 0.", nameof(points));
            if (points * Customer.PointValue > SubTotal - DiscountAmount)
                throw new InvalidOperationException("Số điểm quy đổi vượt quá số tiền cần thanh toán.");
            AppliedPoints = points;
        }

        public void SetPaymentMethod(PaymentType method) => PaymentMethod = method;

        public void SetCustomer(string? customerId)
        {
            EnsureNoDiscountApplied();
            CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId;
        }

        public decimal CalculateChange(decimal cashGiven)
        {
            if (cashGiven < FinalAmount) throw new ArgumentException("Tiền khách đưa chưa đủ.", nameof(cashGiven));
            return cashGiven - FinalAmount;
        }

        public void Validate()
        {
            if (_items.Count == 0) throw new InvalidOperationException("Hóa đơn chưa có sản phẩm.");
            if (FinalAmount < 0) throw new InvalidOperationException("Số tiền thanh toán không hợp lệ.");
        }

        public void AssignId(int billId)
        {
            if (BillId != 0) throw new InvalidOperationException("Mã hóa đơn đã được gán.");
            BillId = billId;
            foreach (var item in _items) item.AssignBill(billId);
        }

        public void Restore(string? promotionId, decimal discountAmount, int appliedPoints)
        {
            PromotionId = promotionId;
            DiscountAmount = discountAmount;
            AppliedPoints = appliedPoints;
        }

        private void EnsureNoDiscountApplied()
        {
            if (DiscountAmount > 0 || AppliedPoints > 0)
                throw new InvalidOperationException("Hãy gỡ khuyến mãi/điểm trước khi thay đổi hóa đơn.");
        }
    }
}
