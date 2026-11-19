using System;
using System.Text.RegularExpressions;

namespace SupermarketManagementSystem
{
    public abstract class Person
    {
        private static readonly Regex PhoneRegex = new Regex(@"^0\d{9}$");

        public string Id { get; private set; }
        public string FullName { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Address { get; private set; }

        protected Person(string id, string fullName, string phoneNumber, string address)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Mã định danh không được để trống.", nameof(id));
            Id = id.Trim();
            UpdateInfo(fullName, phoneNumber, address);
        }

        public void UpdateInfo(string fullName, string phoneNumber, string address)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Họ tên không được để trống.", nameof(fullName));
            if (string.IsNullOrWhiteSpace(phoneNumber) || !PhoneRegex.IsMatch(phoneNumber.Trim()))
                throw new ArgumentException("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.", nameof(phoneNumber));

            FullName = fullName.Trim();
            PhoneNumber = phoneNumber.Trim();
            Address = address?.Trim() ?? string.Empty;
        }

        public abstract string GetDisplayInfo();

        public override string ToString() => GetDisplayInfo();
    }
}
