using System;

namespace SupermarketManagementSystem
{
    public abstract class BaseService
    {
        protected readonly ISessionContext Session;

        protected BaseService(ISessionContext session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected void EnsureAuthenticated()
        {
            if (!Session.IsAuthenticated || Session.CurrentUser == null)
                throw new PermissionDeniedException("Vui lòng đăng nhập để thực hiện thao tác.");
        }

        protected void RequireManagement()
        {
            EnsureAuthenticated();
            Session.RequireManagement();
        }

        protected void RequireInventoryAccess()
        {
            EnsureAuthenticated();
            Session.RequireInventoryAccess();
        }

        protected void RequireAuthenticatedRead()
        {
            EnsureAuthenticated();
        }
    }

    // BR08: Thực thi thao tác trong một transaction, lỗi bất kỳ sẽ Rollback toàn bộ
    public abstract class TransactionalService : BaseService
    {
        protected readonly IUnitOfWork UnitOfWork;

        protected TransactionalService(ISessionContext session, IUnitOfWork unitOfWork)
            : base(session)
        {
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        protected void ExecuteInTransaction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            UnitOfWork.BeginTransaction();
            try
            {
                action();
                UnitOfWork.Commit();
            }
            catch
            {
                try
                {
                    UnitOfWork.Rollback();
                }
                catch
                {
                    // Tránh che lấp ngoại lệ gốc nếu Rollback gặp sự cố
                }
                throw;
            }
        }

        protected T ExecuteInTransaction<T>(Func<T> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            UnitOfWork.BeginTransaction();
            try
            {
                T result = func();
                UnitOfWork.Commit();
                return result;
            }
            catch
            {
                try
                {
                    UnitOfWork.Rollback();
                }
                catch
                {
                    // Tránh che lấp ngoại lệ gốc nếu Rollback gặp sự cố
                }
                throw;
            }
        }
    }
}