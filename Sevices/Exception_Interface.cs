using System;
using System.Collections.Generic;

namespace SupermarketManagementSystem
{
    // Ngoại lệ vi phạm quy tắc nghiệp vụ
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
        public BusinessRuleException(string message, Exception innerException) : base(message, innerException) { }
    }

    // Ngoại lệ vi phạm phân quyền truy cập (BR11)
    public class PermissionDeniedException : Exception
    {
        public PermissionDeniedException(string message) : base(message) { }
    }

    // Ngữ cảnh phiên đăng nhập hiện tại (BR11)
    public interface ISessionContext
    {
        Employee? CurrentUser { get; }
        bool IsAuthenticated { get; }
        bool IsManagement { get; }
        bool CanSell { get; }
        bool CanManageInventory { get; }

        void RequireManagement();
        void RequireSeller();
        void RequireInventoryAccess();
    }

    // Unit of Work cho giao dịch nguyên tử (BR08)
    public interface IUnitOfWork : IDisposable
    {
        void BeginTransaction();
        void Commit();
        void Rollback();
    }

    // Các Interface Repository phục vụ UC05 - UC08
    public interface ICategoryRepository
    {
        Category? GetById(string categoryId);
        List<Category> GetAll();
        void Add(Category category);
        void Update(Category category);
    }

    public interface ISupplierRepository
    {
        Supplier? GetById(string supplierId);
        List<Supplier> GetAll();
        void Add(Supplier supplier);
        void Update(Supplier supplier);
    }

    public interface IProductRepository
    {
        Product? GetById(string productId);
        Product? GetByBarcode(string barcode);
        List<Product> GetAll();
        void Add(Product product);
        void Update(Product product);
    }

    public interface IStockReceiptRepository
    {
        void Add(StockReceipt receipt);
        List<StockReceipt> GetAll();
    }

    public interface IStockAdjustmentRepository
    {
        void Add(StockAdjustment adjustment);
        List<StockAdjustment> GetAll();
    }

    public interface IEmployeeRepository
    {
        Employee? GetById(string employeeId);
        Employee? GetByIdentification(string identification);
        Employee? GetByPhoneNumber(string phoneNumber);
        List<Employee> GetAll();
        void Add(Employee employee);
        void Update(Employee employee);
    }

    public interface IBillRepository
    {
        List<Bill> GetByDateRange(DateTime startDate, DateTime endDate);
        List<Bill> GetAll();
    }
}