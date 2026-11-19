using System;

namespace SupermarketManagementSystem
{
    public class CartItem
    {
        public Product Product { get; }
        public int SelectedQuantity { get; private set; }
        public decimal LineTotal => Product.SellingPrice * SelectedQuantity;

        public CartItem(Product product, int selectedQuantity)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            ChangeQuantity(selectedQuantity);
        }

        /// <summary>Áp dụng BR05 ngay khi chọn hàng.</summary>
        public void ChangeQuantity(int quantity)
        {
            if (!Product.CanSell(quantity))
                throw new InvalidOperationException($"Không thể bán {quantity} '{Product.Name}': hết hàng hoặc ngừng kinh doanh.");
            SelectedQuantity = quantity;
        }
    }
}
