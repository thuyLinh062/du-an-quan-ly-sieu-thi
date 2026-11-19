using System;
using System.Text.RegularExpressions;

namespace SupermarketManagementSystem
{
    public class Employee : Person
    {
        private static readonly Regex IdentificationRegex = new Regex(@"^(\d{9}|\d{12})$");
        public const int MinWorkingAge = 18;

        public DateTime Birthday { get; private set; }
        public GenderType Gender { get; private set; }
        public string Identification { get; private set; }
        public EmployeeRole Role { get; private set; }
        public decimal BaseSalary { get; private set; }
        public bool IsWorking { get; private set; }

        public Employee(string id, string fullName, string phoneNumber, string address,
                        DateTime birthday, GenderType gender, string identification,
                        EmployeeRole role, decimal baseSalary, bool isWorking = true)
            : base(id, fullName, phoneNumber, address)
        {
            SetBirthday(birthday);
            SetIdentification(identification);
            SetSalary(baseSalary);
            Gender = gender;
            Role = role;
            IsWorking = isWorking;
        }

        public void SetBirthday(DateTime birthday)
        {
            int age = DateTime.Today.Year - birthday.Year;
            if (birthday.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < MinWorkingAge)
                throw new ArgumentException($"Nhân viên phải đủ {MinWorkingAge} tuổi.", nameof(birthday));
            Birthday = birthday;
        }

        public void SetIdentification(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification) || !IdentificationRegex.IsMatch(identification.Trim()))
                throw new ArgumentException("CMND/CCCD phải gồm 9 hoặc 12 chữ số.", nameof(identification));
            Identification = identification.Trim();
        }

        public void SetSalary(decimal salary)
        {
            if (salary < 0) throw new ArgumentException("Lương cơ bản không được âm.", nameof(salary));
            BaseSalary = salary;
        }

        public bool CanLogin(string inputPass)
        {
            if (!IsWorking) return false;
            if (string.IsNullOrEmpty(inputPass)) return false;
            return Identification == inputPass;
        }

        public void ChangeGender(GenderType gender) => Gender = gender;

        public void ChangeRole(EmployeeRole newRole) => Role = newRole;

        public void UpdateSalary(decimal salary) => SetSalary(salary);

        public void Resign()
        {
            if (!IsWorking) throw new InvalidOperationException("Nhân viên đã nghỉ việc.");
            IsWorking = false;
        }

        public bool IsManagement => Role == EmployeeRole.Manager;
        public bool CanSell => IsWorking && (Role == EmployeeRole.Cashier || IsManagement);
        public bool CanManageInventory => IsWorking && (Role == EmployeeRole.WarehouseStaff || IsManagement);

        public override string GetDisplayInfo() => $"[{Role}] {Id} - {FullName}";
    }
}
