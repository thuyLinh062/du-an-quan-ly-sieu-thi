using System;
using System.Collections.Generic;
using SupermarketManagementSystem.Entities;
using SupermarketManagementSystem.Enums;
using SupermarketManagementSystem.Exceptions;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class CustomerService : BaseService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ISessionContext sessionContext, ICustomerRepository customerRepository)
            : base(sessionContext)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        /// <summary>
        /// UC02: Tìm khách hàng thân thiết theo số điện thoại
        /// </summary>
        public Customer FindByPhone(string phone)
        {
            SessionContext.RequireSeller(); // BR12: Thu ngân hoặc Quản lý

            if (string.IsNullOrWhiteSpace(phone))
                throw new BusinessRuleException("Vui lòng nhập số điện thoại cần tìm.");

            return _customerRepository.GetByPhone(phone.Trim());
        }

        /// <summary>
        /// UC02: Lấy danh sách toàn bộ khách hàng thành viên
        /// </summary>
        public List<Customer> GetAllCustomers()
        {
            SessionContext.RequireSeller(); // BR12
            return _customerRepository.GetAll();
        }

        /// <summary>
        /// UC02: Tạo nhanh khách hàng thành viên mới
        /// </summary>
        public bool CreateCustomer(Customer customer)
        {
            SessionContext.RequireSeller(); // BR12

            if (customer == null)
                throw new BusinessRuleException("Thông tin khách hàng không hợp lệ.");

            if (string.IsNullOrWhiteSpace(customer.FullName) || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                throw new BusinessRuleException("Họ tên và số điện thoại khách hàng không được để trống.");

            if (_customerRepository.CheckPhoneExists(customer.PhoneNumber.Trim()))
                throw new BusinessRuleException("Số điện thoại này đã được đăng ký cho khách hàng khác.");

            // Khởi tạo giá trị mặc định cho thành viên mới
            customer.PhoneNumber = customer.PhoneNumber.Trim();
            customer.ShoppingPoints = 0;
            customer.TotalSpent = 0;
            customer.Tier = CustomerTier.Standard;

            return _customerRepository.Insert(customer);
        }

        /// <summary>
        /// UC02: Cập nhật thông tin hồ sơ khách hàng
        /// </summary>
        public bool UpdateCustomer(Customer customer)
        {
            SessionContext.RequireSeller(); // BR12

            if (customer == null || string.IsNullOrWhiteSpace(customer.Id))
                throw new BusinessRuleException("Không xác định được khách hàng cần cập nhật.");

            Customer existing = _customerRepository.GetById(customer.Id);
            if (existing == null)
                throw new BusinessRuleException("Khách hàng không tồn tại trong hệ thống.");

            // Kiểm tra trùng SĐT nếu có thay đổi số điện thoại
            if (!string.Equals(existing.PhoneNumber, customer.PhoneNumber?.Trim(), StringComparison.OrdinalIgnoreCase)
                && _customerRepository.CheckPhoneExists(customer.PhoneNumber.Trim()))
            {
                throw new BusinessRuleException("Số điện thoại mới đã trùng với khách hàng khác.");
            }

            existing.FullName = customer.FullName?.Trim();
            existing.PhoneNumber = customer.PhoneNumber?.Trim();
            existing.Address = customer.Address?.Trim();

            return _customerRepository.Update(existing);
        }

        /// <summary>
        /// UC02 & BR12: Điều chỉnh điểm tích lũy thủ công (Chỉ Quản lý được phép gọi)
        /// </summary>
        public bool AdjustPoints(string custId, int pointDelta)
        {
            // BR12: Thu ngân gọi hàm này sẽ ném PermissionDeniedException
            SessionContext.RequireManagement();

            if (pointDelta == 0)
                throw new BusinessRuleException("Số điểm điều chỉnh phải khác 0.");

            Customer customer = _customerRepository.GetById(custId);
            if (customer == null)
                throw new BusinessRuleException("Không tìm thấy khách hàng.");

            if (customer.ShoppingPoints + pointDelta < 0)
                throw new BusinessRuleException("Số điểm tích lũy sau điều chỉnh không được nhỏ hơn 0.");

            customer.ShoppingPoints += pointDelta;
            return _customerRepository.Update(customer);
        }
    }
}
