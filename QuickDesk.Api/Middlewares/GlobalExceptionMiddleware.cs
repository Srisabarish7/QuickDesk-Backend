using QuickDesk.Domain.Common.ExceptionHandling;
using QuickDesk.Domain.Common.Response;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json;

namespace QuickDesk.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (EnableBuffering(context))
            {
                context.Request.EnableBuffering();
            }

            try
            {
                await _next(context);
                await HandlePipelineErrorAsync(context);
            }
            catch (Exception ex)
            {
                await LogAndWriteAsync(context, ex);
            }
        }

        private bool EnableBuffering(HttpContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(context.Request.ContentType))
                {
                    return false;
                }

                if ((context.Request.Method == HttpMethods.Post ||
                    context.Request.Method == HttpMethods.Patch ||
                    context.Request.Method == HttpMethods.Put) &&
                    (context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                     context.Request.ContentType.Contains("application/www-form-urlencoded", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if request buffering should be enabled.");
                return false;
            }
        }

        private async Task HandlePipelineErrorAsync(HttpContext context)
        {
            var statsuMap = new Dictionary<int, HttpStatusCode>
            {
                {StatusCodes.Status401Unauthorized, HttpStatusCode.Unauthorized },
                {StatusCodes.Status403Forbidden, HttpStatusCode.Forbidden  },
                {StatusCodes.Status404NotFound, HttpStatusCode.NotFound },
                {StatusCodes.Status500InternalServerError, HttpStatusCode.InternalServerError }
            };

            if (statsuMap.TryGetValue(context.Response.StatusCode, out var statusCode))
            {
                _logger.LogError("----- {StatusCode} {StatusDescription} Response returned by the pipeline ---- \nPath: {Path} ", (int)statusCode, statusCode, context.Request.Path);
                await WriteErrorResponseAsync(context, statusCode);
            }
        }

        private async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, List<string> CustomErrors = null)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the error response will not be written.");
                return;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var errorDetails = CustomErrors.Count > 0 ? string.Join(", ", CustomErrors) : string.Empty;

            var errorResponse = new ApiErrorResponse
            {
                Errors = ApiErrorHandler.GetDefaultMessage(statusCode, errorDetails)
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse, options);

            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task LogAndWriteAsync(HttpContext context, Exception ex)
        {
            var (statusCode, customErrors) = ex switch
            {
               BadRequestCustomException badRequestEx => (HttpStatusCode.BadRequest, badRequestEx.Errors),
                UnauthorizedCustomException unauthorizedEx => (HttpStatusCode.Unauthorized, unauthorizedEx.Errors),
                BadHttpRequestException badHttpRequestEx => (HttpStatusCode.BadRequest, null),
                UnauthorizedAccessException unauthorizedAccessEx => (HttpStatusCode.Unauthorized, null),
                _ => (HttpStatusCode.BadRequest, null)
            };

            await LogRequestAsync(context, ex);
            await WriteErrorResponseAsync(context, statusCode, customErrors);   
        }

        private async Task LogRequestAsync(HttpContext context, Exception ex)
        {
            var RoutValue = context.Request.RouteValues.Select(r => $"{r.Key}={r.Value}").ToList();
            var queryParams = context.Request.Query.Select(q => $"{q.Key}={q.Value}").ToList();

            var requestBody = string.Empty;
            var requestBodyContent = string.Empty;

            try
            {
                if (context.Request.HasFormContentType)
                {
                    requestBody = "FormData";
                    var formDictonary = context.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

                    requestBodyContent = formDictonary.Count > 0 ? JsonConvert.SerializeObject(formDictonary, Formatting.Indented) : "[Empty Body]";
                }
                else if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    requestBody = "JsonData";
                    var jsonBody = await RequestReadAsync(context);
                    if (string.IsNullOrWhiteSpace(jsonBody))
                    {
                        requestBodyContent = "[Empty Body]";
                    }
                    else
                    {
                        requestBodyContent = jsonBody;
                    }
                }
                else
                {
                    requestBody = "Unsupported or Empty Body";
                    requestBodyContent = $"[No supported body content. ContentType: {context.Request.ContentType} ?? None]";
                }
            }
            catch (Exception formEx)
            {
                requestBody = "Error reading form data";
                requestBodyContent = "[Error reading form data]";
                _logger.LogError(formEx, "Error occurred while reading form data from the request.");

                var reseponseMessage = "--------Incoming Request--------\n" +
                                       "Method: {Method}\n" +
                                       "Path: {Path}\n" +
                                       "Route: {Route}\n" +
                                       "Query: {Query}\n" +
                                       "Body: {Body}\n" +
                                       "BodyContent: {BodyContent}\n" +
                                       "-------------------------------";
                if (ex != null)
                {
                    _logger.LogError(ex, reseponseMessage, context.Request.Method, context.Request.Path, string.Join(", ", RoutValue), string.Join(", ", queryParams), requestBody, requestBodyContent);
                }
                else
                {
                    _logger.LogInformation(reseponseMessage, context.Request.Method, context.Request.Path, string.Join(", ", RoutValue), string.Join(", ", queryParams), requestBody, requestBodyContent);

                }
            }
        }

        private async Task<string> RequestReadAsync(HttpContext context)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if(body.Length > 5000)
            {
                body = body.Substring(0, 5000) + "... [truncated]";
            }

            return string.IsNullOrWhiteSpace(body) ? "[Empty Body]" : body;
        }
    }
}