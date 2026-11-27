using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketManagementSystem
{
    public class EmployeeService : BaseService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(ISessionContext session, IEmployeeRepository employeeRepository)
            : base(session)
        {
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        public void AddEmployee(
            string id,
            string fullName,
            string phoneNumber,
            string address,
            DateTime birthday,
            GenderType gender,
            string identification,
            EmployeeRole role,
            decimal baseSalary,
            bool isWorking = true)
        {
            RequireManagement();

            string cleanId = id?.Trim() ?? string.Empty;
            string cleanIdentification = identification?.Trim() ?? string.Empty;
            string cleanPhone = phoneNumber?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(cleanId) && _employeeRepository.GetById(cleanId) != null)
                throw new BusinessRuleException($"Mã nhân viên '{cleanId}' đã tồn tại.");

            if (!string.IsNullOrEmpty(cleanIdentification) && _employeeRepository.GetByIdentification(cleanIdentification) != null)
                throw new BusinessRuleException($"Số CCCD/CMND '{cleanIdentification}' đã tồn tại.");

            if (!string.IsNullOrEmpty(cleanPhone) && _employeeRepository.GetByPhoneNumber(cleanPhone) != null)
                throw new BusinessRuleException($"Số điện thoại '{cleanPhone}' đã tồn tại.");

            try
            {
                var employee = new Employee(
                    cleanId,
                    fullName,
                    cleanPhone,
                    address,
                    birthday,
                    gender,
                    cleanIdentification,
                    role,
                    baseSalary,
                    isWorking);

                _employeeRepository.Add(employee);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public void UpdateEmployee(
            string id,
            string fullName,
            string phoneNumber,
            string address,
            DateTime birthday,
            GenderType gender,
            string identification,
            EmployeeRole role,
            decimal baseSalary)
        {
            RequireManagement();

            string cleanId = id?.Trim() ?? string.Empty;
            var employee = _employeeRepository.GetById(cleanId);
            if (employee == null)
                throw new BusinessRuleException($"Không tìm thấy nhân viên '{cleanId}'.");

            string cleanIdentification = identification?.Trim() ?? string.Empty;
            string cleanPhone = phoneNumber?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(cleanIdentification))
            {
                var dupIdCard = _employeeRepository.GetByIdentification(cleanIdentification);
                if (dupIdCard != null && !string.Equals(dupIdCard.Id, employee.Id, StringComparison.OrdinalIgnoreCase))
                    throw new BusinessRuleException($"Số CCCD/CMND '{cleanIdentification}' đã được sử dụng bởi nhân viên khác.");
            }

            if (!string.IsNullOrEmpty(cleanPhone))
            {
                var dupPhone = _employeeRepository.GetByPhoneNumber(cleanPhone);
                if (dupPhone != null && !string.Equals(dupPhone.Id, employee.Id, StringComparison.OrdinalIgnoreCase))
                    throw new BusinessRuleException($"Số điện thoại '{cleanPhone}' đã được sử dụng bởi nhân viên khác.");
            }

            bool wasManagement = employee.Role == EmployeeRole.Manager;
            bool willBeManagement = role == EmployeeRole.Manager;
            if (employee.IsWorking && wasManagement && !willBeManagement)
            {
                EnsureNotLastActiveManagement(employee.Id);
            }

            try
            {
                employee.UpdateInfo(fullName, cleanPhone, address);
                employee.SetBirthday(birthday);
                employee.SetIdentification(cleanIdentification);
                employee.UpdateSalary(baseSalary);
                employee.ChangeGender(gender);;

                if (employee.Role != role)
                {
                    employee.ChangeRole(role);
                }

                _employeeRepository.Update(employee);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public void ResignEmployee(string id)
        {
            RequireManagement();

            string cleanId = id?.Trim() ?? string.Empty;
            var employee = _employeeRepository.GetById(cleanId);
            if (employee == null)
                throw new BusinessRuleException($"Không tìm thấy nhân viên '{cleanId}'.");

            if (Session.CurrentUser != null &&
                string.Equals(Session.CurrentUser.Id, employee.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException("Không thể tự khóa/cho nghỉ việc tài khoản đang đăng nhập.");
            }

            if (employee.IsWorking &&
                (employee.Role == EmployeeRole.Manager))
            {
                EnsureNotLastActiveManagement(employee.Id);
            }

            try
            {
                employee.Resign();
                _employeeRepository.Update(employee);
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public List<Employee> GetAllEmployees()
        {
            RequireManagement();
            return _employeeRepository.GetAll();
        }

        private void EnsureNotLastActiveManagement(string targetEmployeeId)
        {
            int activeManagementCount = _employeeRepository.GetAll()
                .Count(e => e.IsWorking &&
                            (e.Role == EmployeeRole.Manager) &&
                            !string.Equals(e.Id, targetEmployeeId, StringComparison.OrdinalIgnoreCase));

            if (activeManagementCount == 0)
                throw new BusinessRuleException("Không thể vô hiệu hóa hoặc hạ quyền Quản lý hoạt động cuối cùng của hệ thống.");
        }
    }
}