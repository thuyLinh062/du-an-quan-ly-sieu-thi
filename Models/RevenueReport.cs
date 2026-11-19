using System;

namespace SupermarketManagementSystem
{
    public class RevenueReport
    {
        public DateTime ReportDate { get; }
        public int TotalBills { get; }
        public decimal TotalRevenue { get; }
        public int TotalPointsUsed { get; }
        public int UniqueCustomers { get; }
        public decimal AverageBillValue => TotalBills == 0 ? 0 : TotalRevenue / TotalBills;

        public RevenueReport(DateTime reportDate, int totalBills, decimal totalRevenue,
                             int totalPointsUsed, int uniqueCustomers)
        {
            if (totalBills < 0 || totalRevenue < 0 || totalPointsUsed < 0 || uniqueCustomers < 0)
                throw new ArgumentException("Số liệu báo cáo không được âm.");
            ReportDate = reportDate;
            TotalBills = totalBills;
            TotalRevenue = totalRevenue;
            TotalPointsUsed = totalPointsUsed;
            UniqueCustomers = uniqueCustomers;
        }
    }
}
