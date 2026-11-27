using System;
using System.Collections.Generic;
using SupermarketManagementSystem.Entities;
using SupermarketManagementSystem.Exceptions;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class PromotionService : BaseService
    {
        private readonly IPromotionRepository _promotionRepository;

        public PromotionService(ISessionContext sessionContext, IPromotionRepository promotionRepository)
            : base(sessionContext)
        {
            _promotionRepository = promotionRepository ?? throw new ArgumentNullException(nameof(promotionRepository));
        }

        /// <summary>
        /// UC04: Lấy toàn bộ danh sách chương trình khuyến mãi
        /// </summary>
        public List<Promotion> GetAllPromotions()
        {
            SessionContext.RequireSeller(); // Thu ngân & Quản lý đều xem được
            return _promotionRepository.GetAll();
        }

        /// <summary>
        /// UC04: Tạo mới chương trình khuyến mãi (Chỉ Quản lý)
        /// </summary>
        public bool CreatePromotion(Promotion promo)
        {
            SessionContext.RequireManagement(); // BR12

            ValidatePromotionData(promo);

            if (_promotionRepository.GetById(promo.PromotionId.Trim()) != null)
                throw new BusinessRuleException($"Mã khuyến mãi '{promo.PromotionId}' đã tồn tại.");

            promo.PromotionId = promo.PromotionId.Trim().ToUpper();
            promo.Name = promo.Name.Trim();
            return _promotionRepository.Insert(promo);
        }

        /// <summary>
        /// UC04: Cập nhật thông tin chương trình khuyến mãi (Chỉ Quản lý)
        /// </summary>
        public bool UpdatePromotion(Promotion promo)
        {
            SessionContext.RequireManagement(); // BR12

            ValidatePromotionData(promo);

            Promotion existing = _promotionRepository.GetById(promo.PromotionId);
            if (existing == null)
                throw new BusinessRuleException("Chương trình khuyến mãi không tồn tại.");

            existing.Name = promo.Name.Trim();
            existing.DiscountPercent = promo.DiscountPercent;
            existing.MinOrderAmount = promo.MinOrderAmount;
            existing.StartDate = promo.StartDate;
            existing.EndDate = promo.EndDate;
            existing.IsActive = promo.IsActive;

            return _promotionRepository.Update(existing);
        }

        /// <summary>
        /// UC04: Kích hoạt chương trình khuyến mãi (Chỉ Quản lý)
        /// </summary>
        public bool ActivatePromotion(string promoId)
        {
            SessionContext.RequireManagement(); // BR12
            return SetPromotionStatus(promoId, true);
        }

        /// <summary>
        /// UC04: Vô hiệu hóa chương trình khuyến mãi (Chỉ Quản lý)
        /// </summary>
        public bool DeactivatePromotion(string promoId)
        {
            SessionContext.RequireManagement(); // BR12
            return SetPromotionStatus(promoId, false);
        }

        /// <summary>
        /// UC04 & BR04: Kiểm tra tính hợp lệ của mã khuyến mãi khi áp dụng vào đơn hàng
        /// </summary>
        public Promotion ValidatePromotion(string promoCode, decimal orderAmount)
        {
            SessionContext.RequireSeller(); // BR12: Thu ngân & Quản lý đều kiểm tra được

            if (string.IsNullOrWhiteSpace(promoCode))
                throw new BusinessRuleException("Vui lòng nhập mã khuyến mãi.");

            Promotion promo = _promotionRepository.GetById(promoCode.Trim());
            if (promo == null)
                throw new BusinessRuleException("Mã khuyến mãi không tồn tại.");

            // BR04: Kiểm tra IsActive == true, trong khoảng [StartDate, EndDate] và orderAmount >= MinOrderAmount
            if (!promo.IsValid(orderAmount, DateTime.Now))
                throw new BusinessRuleException("Mã khuyến mãi đã hết hạn, ngừng áp dụng hoặc chưa đạt giá trị đơn tối thiểu.");

            return promo;
        }

        private bool SetPromotionStatus(string promoId, bool isActive)
        {
            Promotion promo = _promotionRepository.GetById(promoId);
            if (promo == null)
                throw new BusinessRuleException("Không tìm thấy chương trình khuyến mãi.");

            promo.IsActive = isActive;
            return _promotionRepository.Update(promo);
        }

        private void ValidatePromotionData(Promotion promo)
        {
            if (promo == null)
                throw new BusinessRuleException("Dữ liệu khuyến mãi không hợp lệ.");

            if (string.IsNullOrWhiteSpace(promo.PromotionId) || string.IsNullOrWhiteSpace(promo.Name))
                throw new BusinessRuleException("Mã và tên chương trình khuyến mãi không được để trống.");

            if (promo.EndDate < promo.StartDate)
                throw new BusinessRuleException("Ngày kết thúc không được nhỏ hơn ngày bắt đầu.");

            // Hỗ trợ cả định dạng nhập 0.1 (10%) hoặc (0, 100] tùy quy ước Entity
            if (promo.DiscountPercent <= 0 || promo.DiscountPercent > 100)
                throw new BusinessRuleException("Tỷ lệ giảm giá phải nằm trong khoảng (0, 100].");

            if (promo.MinOrderAmount < 0)
                throw new BusinessRuleException("Giá trị hóa đơn tối thiểu không được âm.");
        }
    }
}
