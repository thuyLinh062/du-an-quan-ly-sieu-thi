using System;
using System.Collections.Generic;

namespace SupermarketManagementSystem
{
    // Các Interface Repository 
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