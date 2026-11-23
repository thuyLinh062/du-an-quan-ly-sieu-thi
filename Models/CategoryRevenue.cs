using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketManagementSystem
{
    public class CategoryRevenue
    {
        public string CategoryId { get; private set; }
        public string CategoryName { get; private set; }
        public decimal Revenue { get; private set; }

        public CategoryRevenue(string categoryId, string categoryName, decimal revenue)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                throw new ArgumentException("Mã danh mục trống.", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentException("Tên danh mục trống.", nameof(categoryName));
            if (revenue < 0)
                throw new ArgumentException("Doanh thu danh mục không được âm.", nameof(revenue));

            CategoryId = categoryId.Trim();
            CategoryName = categoryName.Trim();
            Revenue = revenue;
        }
    }
}