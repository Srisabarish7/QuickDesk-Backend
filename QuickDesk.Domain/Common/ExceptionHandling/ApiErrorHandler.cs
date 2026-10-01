using QuickDesk.Domain.Common.Response;
using System.Net;

namespace QuickDesk.Domain.Common.ExceptionHandling
{
    public class ApiErrorHandler
    {
        public static ApiError GetDefaultMessage(HttpStatusCode statusCode, string customMesssage = null)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => new ApiError { Message = "Bad Request", Details = String.IsNullOrWhiteSpace(customMesssage) ? HttpErrorMessage.BadRequest.ToString() : customMesssage },
                HttpStatusCode.Unauthorized => new ApiError { Message = "Unauthorized", Details = String.IsNullOrWhiteSpace(customMesssage) ? HttpErrorMessage.Unauthorized.ToString() : customMesssage },
                HttpStatusCode.Forbidden => new ApiError { Message = "Forbidden", Details = String.IsNullOrWhiteSpace(customMesssage) ? HttpErrorMessage.Forbidden.ToString() : customMesssage },
                HttpStatusCode.NotFound => new ApiError { Message = "Not Found", Details = String.IsNullOrWhiteSpace(customMesssage) ? HttpErrorMessage.NotFound.ToString() : customMesssage },
                HttpStatusCode.InternalServerError => new ApiError { Message = "Internal Server Error", Details = String.IsNullOrWhiteSpace(customMesssage) ? HttpErrorMessage.InternalServerError.ToString() : customMesssage },
                _ => new ApiError { Message = "Unexpected Error", Details = String.IsNullOrWhiteSpace(customMesssage) ? HttpErrorMessage.UnexpectedError.ToString() : customMesssage }
            };
        }
    }
}
