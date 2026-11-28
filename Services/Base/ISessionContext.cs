using System;
using System.Collections.Generic;

namespace SupermarketManagementSystem
{
    // Ngữ cảnh phiên đăng nhập hiện tại (BR12)
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
}