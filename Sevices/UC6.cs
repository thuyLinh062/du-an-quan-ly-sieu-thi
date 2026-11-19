using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketManagementSystem
{

    public class StockAdjustment
    {
        public string AdjustmentId { get; private set; }
        public string ProductId { get; private set; }
        public string EmployeeId { get; private set; }
        public int QuantityChange { get; private set; }
        public AdjustmentReason Reason { get; private set; }
        public string Note { get; private set; }
        public DateTime AdjustedAt { get; private set; }

        public StockAdjustment(
            string adjustmentId,
            string productId,
            string employeeId,
            int quantityChange,
            AdjustmentReason reason,
            string note,
            DateTime? adjustedAt = null)
        {
            if (string.IsNullOrWhiteSpace(adjustmentId))
                throw new ArgumentException("Mã phiếu điều chỉnh trống.", nameof(adjustmentId));
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Thiếu mã sản phẩm.", nameof(productId));
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Thiếu mã nhân viên thực hiện.", nameof(employeeId));
            if (string.IsNullOrWhiteSpace(note))
                throw new BusinessRuleException("Bắt buộc nhập ghi chú (Note) khi điều chỉnh tồn kho.");
            if (quantityChange == 0)
                throw new BusinessRuleException("Mức thay đổi tồn kho (QuantityChange) phải khác 0.");

            if ((reason == AdjustmentReason.Damaged ||
                 reason == AdjustmentReason.Expired ||
                 reason == AdjustmentReason.Lost) && quantityChange >= 0)
            {
                throw new BusinessRuleException($"Lý do {reason} bắt buộc làm giảm tồn kho (QuantityChange < 0).");
            }

            AdjustmentId = adjustmentId.Trim();
            ProductId = productId.Trim();
            EmployeeId = employeeId.Trim();
            QuantityChange = quantityChange;
            Reason = reason;
            Note = note.Trim();
            AdjustedAt = adjustedAt ?? DateTime.Now;
        }
    }

    public record StockReceiptItemInput(string ProductId, int Quantity, decimal ImportUnitPrice);

    public class InventoryService : TransactionalService
    {
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IStockReceiptRepository _stockReceiptRepository;
        private readonly IStockAdjustmentRepository _stockAdjustmentRepository;

        public InventoryService(
            ISessionContext session,
            IUnitOfWork unitOfWork,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository,
            IStockReceiptRepository stockReceiptRepository,
            IStockAdjustmentRepository stockAdjustmentRepository)
            : base(session, unitOfWork)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
            _stockReceiptRepository = stockReceiptRepository ?? throw new ArgumentNullException(nameof(stockReceiptRepository));
            _stockAdjustmentRepository = stockAdjustmentRepository ?? throw new ArgumentNullException(nameof(stockAdjustmentRepository));
        }

        public StockReceipt CreateStockReceipt(
            string receiptId,
            string supplierId,
            IEnumerable<StockReceiptItemInput> items,
            string note = "",
            DateTime? importDate = null)
        {
            RequireInventoryAccess();

            string employeeId = Session.CurrentUser!.Id;
            string cleanSupplierId = supplierId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cleanSupplierId) || _supplierRepository.GetById(cleanSupplierId) == null)
                throw new BusinessRuleException($"Nhà cung cấp '{cleanSupplierId}' không tồn tại.");

            var itemList = items?.ToList() ?? new List<StockReceiptItemInput>();
            if (itemList.Count == 0)
                throw new BusinessRuleException("Phiếu nhập kho không được rỗng.");

            return ExecuteInTransaction(() =>
            {
                StockReceipt receipt;
                try
                {
                    receipt = new StockReceipt(receiptId, cleanSupplierId, employeeId, importDate, note);
                }
                catch (ArgumentException ex)
                {
                    throw new BusinessRuleException(ex.Message, ex);
                }

                foreach (var item in itemList)
                {
                    string cleanProdId = item.ProductId?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(cleanProdId))
                        throw new BusinessRuleException("Dòng nhập kho thiếu mã sản phẩm.");

                    try
                    {
                        receipt.AddItem(cleanProdId, item.Quantity, item.ImportUnitPrice);
                    }
                    catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                    {
                        throw new BusinessRuleException(ex.Message, ex);
                    }

                    var product = _productRepository.GetById(cleanProdId);
                    if (product == null)
                        throw new BusinessRuleException($"Sản phẩm '{cleanProdId}' không tồn tại.");

                    try
                    {
                        product.IncreaseStock(item.Quantity);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new BusinessRuleException(ex.Message, ex);
                    }

                    _productRepository.Update(product);
                }

                try
                {
                    receipt.Validate();
                }
                catch (InvalidOperationException ex)
                {
                    throw new BusinessRuleException(ex.Message, ex);
                }

                _stockReceiptRepository.Add(receipt);
                return receipt;
            });
        }

        public StockAdjustment AdjustStock(
            string adjustmentId,
            string productId,
            int quantityChange,
            AdjustmentReason reason,
            string note,
            DateTime? adjustedAt = null)
        {
            RequireInventoryAccess();

            string employeeId = Session.CurrentUser!.Id;
            string cleanProdId = productId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cleanProdId))
                throw new BusinessRuleException("Thiếu mã sản phẩm cần điều chỉnh.");

            return ExecuteInTransaction(() =>
            {
                StockAdjustment adjustment;
                try
                {
                    adjustment = new StockAdjustment(
                        adjustmentId,
                        cleanProdId,
                        employeeId,
                        quantityChange,
                        reason,
                        note,
                        adjustedAt);
                }
                catch (ArgumentException ex)
                {
                    throw new BusinessRuleException(ex.Message, ex);
                }

                var product = _productRepository.GetById(cleanProdId);
                if (product == null)
                    throw new BusinessRuleException($"Không tìm thấy sản phẩm '{cleanProdId}'.");

                try
                {
                    product.AdjustStock(quantityChange);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    throw new BusinessRuleException(ex.Message, ex);
                }

                _productRepository.Update(product);
                _stockAdjustmentRepository.Add(adjustment);
                return adjustment;
            });
        }

        public List<Product> GetStockList(bool onlyLowStock = false, int threshold = Product.DefaultLowStockThreshold)
        {
            RequireInventoryAccess();

            var products = _productRepository.GetAll();
            return onlyLowStock
                ? products.Where(p => p.IsLowStock(threshold)).ToList()
                : products;
        }

        public List<Supplier> GetSuppliers()
        {
            RequireInventoryAccess();
            return _supplierRepository.GetAll();
        }

        public void AddSupplier(string supplierId, string companyName, string hotline, string contactPerson, string address)
        {
            RequireInventoryAccess();

            string cleanId = supplierId?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(cleanId) && _supplierRepository.GetById(cleanId) != null)
                throw new BusinessRuleException($"Mã nhà cung cấp '{cleanId}' đã tồn tại.");

            try
            {
                var supplier = new Supplier(cleanId, companyName, hotline, contactPerson, address);
                _supplierRepository.Add(supplier);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }

        public void UpdateSupplier(string supplierId, string companyName, string hotline, string contactPerson, string address)
        {
            RequireInventoryAccess();

            string cleanId = supplierId?.Trim() ?? string.Empty;
            var supplier = _supplierRepository.GetById(cleanId);
            if (supplier == null)
                throw new BusinessRuleException($"Không tìm thấy nhà cung cấp '{cleanId}'.");

            try
            {
                supplier.Update(companyName, hotline, contactPerson, address);
                _supplierRepository.Update(supplier);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessRuleException(ex.Message, ex);
            }
        }
    }
}