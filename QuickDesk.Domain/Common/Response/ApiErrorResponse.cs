using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Domain.Common.Response
{
    public class ApiErrorResponse
    {
        public ApiError Errors { get; set; }
    }

    public class ApiError
    {
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
