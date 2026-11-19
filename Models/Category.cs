using System;

namespace SupermarketManagementSystem
{
    public class Category
    {
        public string CategoryId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }

        public Category(string categoryId, string name, string description = "", bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(categoryId)) throw new ArgumentException("Mã danh mục trống.", nameof(categoryId));
            CategoryId = categoryId.Trim();
            Update(name, description);
            IsActive = isActive;
        }

        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Tên danh mục trống.", nameof(name));
            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
        }

        public void SetActive(bool active) => IsActive = active;

        public override string ToString() => $"{CategoryId} - {Name}";
    }
}
