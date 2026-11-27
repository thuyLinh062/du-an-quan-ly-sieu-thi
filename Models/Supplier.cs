using System;

namespace SupermarketManagementSystem
{
    public class Supplier : IManageable
    {
        public string SupplierId { get; private set; }
        public string CompanyName { get; private set; }
        public string Hotline { get; private set; }
        public string ContactPerson { get; private set; }
        public string Address { get; private set; }
        public bool IsActive { get; private set; }

        public Supplier(string supplierId, string companyName, string hotline, string contactPerson, string address, bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(supplierId)) throw new ArgumentException("Mã NCC trống.", nameof(supplierId));
            SupplierId = supplierId.Trim();
            Update(companyName, hotline, contactPerson, address);
            IsActive = isActive;
        }

        public void Update(string companyName, string hotline, string contactPerson, string address)
        {
            if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Tên công ty trống.", nameof(companyName));
            CompanyName = companyName.Trim();
            Hotline = hotline?.Trim() ?? string.Empty;
            ContactPerson = contactPerson?.Trim() ?? string.Empty;
            Address = address?.Trim() ?? string.Empty;
        }

        public void SetActive(bool active) => IsActive = active;
        public void Disable() => IsActive = false;

        public override string ToString() => $"{SupplierId} - {CompanyName}";
    }
}
