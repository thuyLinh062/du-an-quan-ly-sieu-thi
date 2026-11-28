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
}