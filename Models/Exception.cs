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

    // Ngoại lệ vi phạm phân quyền truy cập (BR12)
    public class PermissionDeniedException : Exception
    {
        public PermissionDeniedException(string message) : base(message) { }
    }
}