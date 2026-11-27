using System;

namespace SupermarketManagementSystem
{
    public class Payment
    {
        public string PaymentId { get; private set; }
        public int BillId { get; private set; }
        public decimal AmountPaid { get; private set; }
        public decimal ChangeAmount { get; private set; }   // = AmountPaid - FinalAmount
        public PaymentType PaymentMethod { get; private set; }
        public DateTime PaymentDate { get; private set; }

        public Payment(string paymentId, int billId, decimal amountPaid, decimal finalAmount,
                       PaymentType paymentMethod, DateTime? paymentDate = null)
        {
            if (string.IsNullOrWhiteSpace(paymentId)) throw new ArgumentException("Mã giao dịch trống.", nameof(paymentId));
            if (billId < 0) throw new ArgumentException("Mã hóa đơn không hợp lệ.", nameof(billId));
            if (amountPaid < finalAmount) throw new ArgumentException("Tiền khách đưa chưa đủ.", nameof(amountPaid));

            PaymentId = paymentId.Trim();
            BillId = billId;
            AmountPaid = amountPaid;
            ChangeAmount = amountPaid - finalAmount;
            PaymentMethod = paymentMethod;
            PaymentDate = paymentDate ?? DateTime.Now;
        }

        public void AssignBill(int billId)
        {
            if (BillId != 0) throw new InvalidOperationException("Giao dịch đã gắn với hóa đơn.");
            if (billId <= 0) throw new ArgumentException("Mã hóa đơn không hợp lệ.", nameof(billId));
            BillId = billId;
        }
    }
}