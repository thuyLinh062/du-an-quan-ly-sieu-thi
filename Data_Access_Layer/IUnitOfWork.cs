using System;
using System.Collections.Generic;

namespace SupermarketManagementSystem
{   
    // Unit of Work cho giao dịch nguyên tử (BR09)
    public interface IUnitOfWork : IDisposable
    {
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}