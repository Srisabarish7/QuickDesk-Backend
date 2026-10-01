using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Domain.Common.ExceptionHandling
{
    public class HttpErrorMessage
    {
        public const string BadRequest = "An error occurred while processing your request. Please try again later.";
        public const string Unauthorized = "You are not authorized to access this resource. Please signin and try again";
        public const string Forbidden = "You do not have permission to access this resource.";
        public const string NotFound = "The requested resource could not be found.";
        public const string InternalServerError = "An internal server error occurred. Please try again later.";
        public const string UnexpectedError = "An unexpected error occurred. Please try again later.";
    }
}
