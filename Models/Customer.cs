using System;

namespace SupermarketManagementSystem
{
    public class Customer : Person
    {
        public const decimal PointEarnUnit = 100000m;  // BR02: 100.000đ = 1 điểm
        public const decimal PointValue = 1000m;       // BR03: 1 điểm = 1.000đ
        public const decimal SilverThreshold = 5000000m;
        public const decimal GoldThreshold = 20000000m;
        public const decimal VipThreshold = 50000000m;

        public int ShoppingPoints { get; private set; }
        public decimal TotalSpent { get; private set; }
        public CustomerTier Tier { get; private set; }

        public Customer(string id, string fullName, string phoneNumber, string address,
                        int shoppingPoints = 0, decimal totalSpent = 0)
            : base(id, fullName, phoneNumber, address)
        {
            if (shoppingPoints < 0) throw new ArgumentException("Điểm không được âm.", nameof(shoppingPoints));
            if (totalSpent < 0) throw new ArgumentException("Tổng chi tiêu không được âm.", nameof(totalSpent));
            ShoppingPoints = shoppingPoints;
            TotalSpent = totalSpent;
            Tier = CalculateTier(totalSpent);
        }

        public int EarnPointsFromBill(decimal finalAmount)
        {
            if (finalAmount < 0) throw new ArgumentException("Số tiền không hợp lệ.", nameof(finalAmount));
            
            decimal earnedDecimal = Math.Floor(finalAmount / PointEarnUnit);

            if (earnedDecimal > int.MaxValue - ShoppingPoints)
                throw new OverflowException("Số điểm tích lũy vượt giới hạn.");
            int earned = (int)earnedDecimal;
            ShoppingPoints += earned;
            AddSpending(finalAmount);
            return earned;
        }

        public decimal RedeemPoints(int points)
        {
            if (points <= 0) throw new ArgumentException("Số điểm sử dụng phải lớn hơn 0.", nameof(points));
            if (points > ShoppingPoints) throw new InvalidOperationException("Số điểm sử dụng vượt quá điểm hiện có.");
            ShoppingPoints -= points;
            return points * PointValue;
        }

        public void AdjustPoints(int delta)
        {
            long newPoints = (long)ShoppingPoints + delta;
            
            if (newPoints < 0) throw new InvalidOperationException("Điều chỉnh làm điểm bị âm.");

            if (newPoints > int.MaxValue) throw new OverflowException("Số điểm vượt giới hạn.");

            ShoppingPoints = (int)newPoints;
        }

        public void AddSpending(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Số tiền không hợp lệ.", nameof(amount));

            if (amount > decimal.MaxValue - TotalSpent) throw new OverflowException("Tổng chi tiêu vượt giới hạn.");
            TotalSpent += amount;
            Tier = CalculateTier(TotalSpent);
        }

        public static CustomerTier CalculateTier(decimal totalSpent)
        {
            if (totalSpent >= VipThreshold) return CustomerTier.VIP;
            if (totalSpent >= GoldThreshold) return CustomerTier.Gold;
            if (totalSpent >= SilverThreshold) return CustomerTier.Silver;
            return CustomerTier.Standard;
        }

        public override string GetDisplayInfo() => $"{FullName} ({PhoneNumber}) - {Tier} - {ShoppingPoints} điểm";
    }
}
