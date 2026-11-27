using System;
using System.Collections.Generic;
using System.Linq;
using SupermarketManagementSystem.DTOs;
using SupermarketManagementSystem.Entities;
using SupermarketManagementSystem.Enums;
using SupermarketManagementSystem.Exceptions;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class SaleService : TransactionalService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IBillRepository _billRepository;

        public SaleService(
            ISessionContext sessionContext,
            IUnitOfWork unitOfWork,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IPromotionRepository promotionRepository,
            IBillRepository billRepository)
            : base(sessionContext, unitOfWork)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _promotionRepository = promotionRepository ?? throw new ArgumentNullException(nameof(promotionRepository));
            _billRepository = billRepository ?? throw new ArgumentNullException(nameof(billRepository));
        }

        /// <summary>
        /// UC03 & BR05: Thêm sản phẩm vào giỏ hàng tạm thời và kiểm tra tồn kho
        /// </summary>
        public void AddItemToCart(List<CartItem> cart, string prodId, int qty)
        {
            SessionContext.RequireSeller(); // BR12

            if (cart == null)
                throw new ArgumentNullException(nameof(cart));
            if (qty <= 0)
                throw new BusinessRuleException("Số lượng mua phải lớn hơn 0.");

            Product product = _productRepository.GetById(prodId);
            if (product == null)
                throw new BusinessRuleException("Sản phẩm không tồn tại.");

            CartItem existingItem = cart.FirstOrDefault(x => x.Product.ProductId == prodId);
            int totalRequestedQty = qty + (existingItem?.SelectedQuantity ?? 0);

            // BR05: Chỉ cho phép bán khi IsActive == true và StockQuantity >= totalRequestedQty
            if (!product.CanSell(totalRequestedQty))
                throw new BusinessRuleException($"Sản phẩm '{product.Name}' đã ngừng bán hoặc không đủ tồn kho (Hiện còn: {product.StockQuantity}).");

            if (existingItem != null)
            {
                existingItem.SelectedQuantity = totalRequestedQty;
                existingItem.Product = product;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Product = product,
                    SelectedQuantity = qty
                });
            }
        }

        /// <summary>
        /// UC03: Xóa hoặc giảm bớt sản phẩm khỏi giỏ hàng
        /// </summary>
        public void RemoveItemFromCart(List<CartItem> cart, string prodId)
        {
            SessionContext.RequireSeller(); // BR12

            if (cart == null) return;
            CartItem item = cart.FirstOrDefault(x => x.Product.ProductId == prodId);
            if (item != null)
            {
                cart.Remove(item);
            }
        }

        /// <summary>
        /// UC03: Khởi tạo đối tượng hóa đơn nháp từ giỏ hàng hiện tại
        /// </summary>
        public Bill BuildBill(List<CartItem> cart, string customerId = null)
        {
            SessionContext.RequireSeller(); // BR12

            if (cart == null || cart.Count == 0)
                throw new BusinessRuleException("Giỏ hàng đang trống, không thể lập hóa đơn.");

            Bill bill = new Bill
            {
                CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId.Trim(),
                EmployeeId = SessionContext.CurrentEmployee.Id,
                Status = BillStatus.Pending,
                CreatedAt = DateTime.Now,
                Items = new List<BillItem>()
            };

            decimal subTotal = 0;
            foreach (CartItem item in cart)
            {
                BillItem billItem = new BillItem
                {
                    ProductId = item.Product.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.SelectedQuantity,
                    UnitPrice = item.Product.SellingPrice
                };
                bill.AddItem(billItem);
                subTotal += billItem.Quantity * billItem.UnitPrice;
            }

            bill.SubTotal = subTotal;
            bill.DiscountAmount = 0;
            bill.AppliedPoints = 0;
            bill.FinalAmount = subTotal;

            return bill;
        }

        /// <summary>
        /// UC03, BR03 & BR04: Áp dụng mã khuyến mãi (tính trước) và điểm tích lũy (tính sau)
        /// </summary>
        public Bill ApplyDiscountAndPoints(Bill bill, string promoCode, int usedPoints)
        {
            SessionContext.RequireSeller(); // BR12

            if (bill == null)
                throw new BusinessRuleException("Hóa đơn không hợp lệ.");

            // BR13: Từ chối mọi chỉnh sửa nếu hóa đơn đã thanh toán
            if (bill.IsPaid)
                throw new BusinessRuleException("Hóa đơn đã thanh toán là bất biến, không thể sửa đổi.");

            bill.PromotionId = null;
            bill.DiscountAmount = 0;
            bill.AppliedPoints = 0;

            // BR04: Áp dụng 1 mã khuyến mãi trước khi trừ điểm
            if (!string.IsNullOrWhiteSpace(promoCode))
            {
                Promotion promo = _promotionRepository.GetById(promoCode.Trim());
                if (promo == null || !promo.IsValid(bill.SubTotal, DateTime.Now))
                    throw new BusinessRuleException("Mã khuyến mãi không tồn tại, đã hết hạn hoặc chưa đạt giá trị đơn tối thiểu.");

                bill.ApplyPromotion(promo);
            }

            // BR03: Quy đổi điểm tích lũy (1 điểm = 1.000 VNĐ)
            if (usedPoints > 0)
            {
                if (string.IsNullOrWhiteSpace(bill.CustomerId))
                    throw new BusinessRuleException("Khách vãng lai không thể sử dụng điểm tích lũy.");

                Customer customer = _customerRepository.GetById(bill.CustomerId);
                if (customer == null)
                    throw new BusinessRuleException("Không tìm thấy thông tin khách hàng thành viên.");

                // Kiểm tra UsedPoints <= ShoppingPoints và (UsedPoints * 1000) <= SubTotal
                customer.RedeemPoints(usedPoints, bill.SubTotal);
                bill.ApplyPoints(usedPoints);
            }

            bill.CalculateFinalAmount();
            return bill;
        }

        /// <summary>
        /// UC03, BR02, BR05, BR09, BR10 & BR13: Thanh toán hóa đơn trong một Database Transaction (ACID)
        /// </summary>
        public Bill Checkout(Bill bill, Payment payment)
        {
            SessionContext.RequireSeller(); // BR12

            if (bill == null || bill.Items == null || bill.Items.Count == 0)
                throw new BusinessRuleException("Hóa đơn không có sản phẩm để thanh toán.");

            if (bill.IsPaid)
                throw new BusinessRuleException("Hóa đơn này đã được thanh toán trước đó (BR13).");

            if (payment == null || payment.AmountPaid < bill.FinalAmount)
                throw new BusinessRuleException("Số tiền khách thanh toán không đủ.");

            // BR09: Thực thi toàn bộ quy trình trong 1 giao dịch nguyên tử ACID
            return ExecuteInTransaction(() =>
            {
                // 1. Kiểm tra lại tồn kho thực tế tại thời điểm chốt đơn và trừ tồn (BR05)
                foreach (BillItem item in bill.Items)
                {
                    Product product = _productRepository.GetById(item.ProductId);
                    if (product == null || !product.CanSell(item.Quantity))
                        throw new BusinessRuleException($"Sản phẩm '{item.ProductName}' không đủ tồn kho để thanh toán.");

                    product.DecreaseStock(item.Quantity);
                    _productRepository.UpdateStock(product.ProductId, product.StockQuantity);
                }

                // 2. Đánh dấu hóa đơn đã thanh toán bất biến (BR13) và lưu Bill + Payment
                payment.BillId = bill.BillId;
                payment.ChangeAmount = payment.AmountPaid - bill.FinalAmount;
                payment.PaymentDate = DateTime.Now;

                bill.MarkAsPaid();
                _billRepository.InsertBillAndPayment(bill, payment);

                // 3. Xử lý trừ điểm đã dùng, cộng điểm mới & xét hạng cho khách thành viên (BR02, BR03, BR10)
                if (!string.IsNullOrWhiteSpace(bill.CustomerId))
                {
                    Customer customer = _customerRepository.GetById(bill.CustomerId);
                    if (customer != null)
                    {
                        if (bill.AppliedPoints > 0)
                        {
                            customer.ShoppingPoints -= bill.AppliedPoints;
                        }

                        customer.EarnPointsFromBill(bill.FinalAmount); // Cộng 1 điểm / 100.000đ
                        customer.AddSpending(bill.FinalAmount);        // Cộng dồn TotalSpent
                        customer.UpdateTier();                         // Xét hạng Standard/Silver/Gold/VIP

                        _customerRepository.Update(customer);
                    }
                }

                return bill;
            });
        }
    }
}
