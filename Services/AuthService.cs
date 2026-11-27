using System;
using SupermarketManagementSystem.Entities;
using SupermarketManagementSystem.Exceptions;
using SupermarketManagementSystem.Repositories;

namespace SupermarketManagementSystem.Services
{
    public class AuthService : BaseService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public AuthService(ISessionContext sessionContext, IEmployeeRepository employeeRepository)
            : base(sessionContext)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        /// <summary>
        /// UC01 & BR01: Xác thực EmployeeId + mật khẩu, kiểm tra IsWorking và khởi tạo ISessionContext
        /// </summary>
        public ISessionContext Login(string empId, string password)
        {
            if (string.IsNullOrWhiteSpace(empId) || string.IsNullOrWhiteSpace(password))
                throw new BusinessRuleException("Vui lòng nhập đầy đủ mã nhân viên và mật khẩu.");

            Employee emp = _employeeRepository.GetById(empId.Trim());
            if (emp == null)
                throw new BusinessRuleException("Tài khoản nhân viên không tồn tại.");

            // BR01: Kiểm tra IsWorking == true và khớp mật khẩu / CCCD
            if (!emp.CanLogin(password))
                throw new BusinessRuleException("Sai mật khẩu hoặc nhân viên đã nghỉ việc.");

            SessionContext.Initialize(emp);
            return SessionContext;
        }

        /// <summary>
        /// UC01: Đăng xuất và xóa phiên làm việc hiện tại
        /// </summary>
        public void Logout()
        {
            SessionContext.Clear();
        }
    }
}
