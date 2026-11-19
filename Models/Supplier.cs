using System;

namespace SupermarketManagementSystem
{
    public class Supplier
    {
        public string SupplierId { get; private set; }
        public string CompanyName { get; private set; }
        public string Hotline { get; private set; }
        public string ContactPerson { get; private set; }
        public string Address { get; private set; }

        public Supplier(string supplierId, string companyName, string hotline, string contactPerson, string address)
        {
            if (string.IsNullOrWhiteSpace(supplierId)) throw new ArgumentException("Mã NCC trống.", nameof(supplierId));
            SupplierId = supplierId.Trim();
            Update(companyName, hotline, contactPerson, address);
        }

        public void Update(string companyName, string hotline, string contactPerson, string address)
        {
            if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Tên công ty trống.", nameof(companyName));
            CompanyName = companyName.Trim();
            Hotline = hotline?.Trim() ?? string.Empty;
            ContactPerson = contactPerson?.Trim() ?? string.Empty;
            Address = address?.Trim() ?? string.Empty;
        }

        public override string ToString() => $"{SupplierId} - {CompanyName}";
    }
}
