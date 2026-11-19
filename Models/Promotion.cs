using System;

namespace SupermarketManagementSystem
{
    public class Promotion
    {
        public string PromotionId { get; private set; }
        public string Name { get; private set; }
        public double DiscountPercent { get; private set; }   // 0 -> 100
        public decimal MinOrderAmount { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsActive { get; private set; }

        public Promotion(string promotionId, string name, double discountPercent, decimal minOrderAmount,
                         DateTime startDate, DateTime endDate, bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(promotionId)) throw new ArgumentException("Mã KM trống.", nameof(promotionId));
            PromotionId = promotionId.Trim();
            Update(name, discountPercent, minOrderAmount, startDate, endDate);
            IsActive = isActive;
        }

        public void Update(string name, double discountPercent, decimal minOrderAmount, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên KM trống.", nameof(name));
            if (discountPercent <= 0 || discountPercent > 100)
                throw new ArgumentException("Phần trăm giảm phải trong khoảng (0, 100].", nameof(discountPercent));
            if (minOrderAmount < 0) throw new ArgumentException("Giá trị tối thiểu không được âm.", nameof(minOrderAmount));
            if (endDate < startDate) throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.", nameof(endDate));

            Name = name.Trim();
            DiscountPercent = discountPercent;
            MinOrderAmount = minOrderAmount;
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>BR04: IsActive, nằm trong [StartDate, EndDate] và đạt giá trị tối thiểu.</summary>
        public bool IsValid(decimal orderAmount, DateTime currentDate)
            => IsActive
               && currentDate.Date >= StartDate.Date
               && currentDate.Date <= EndDate.Date
               && orderAmount >= MinOrderAmount;

        public decimal CalculateDiscount(decimal orderAmount)
            => Math.Round(orderAmount * (decimal)DiscountPercent / 100m, 0);

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public override string ToString() => $"{PromotionId} - {Name} (-{DiscountPercent}%)";
    }
}
