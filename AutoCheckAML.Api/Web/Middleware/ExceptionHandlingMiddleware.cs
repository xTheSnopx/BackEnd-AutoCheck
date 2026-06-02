using AutoCheckAML.Api.Helpers.Exceptions;
using AutoCheckAML.Api.Helpers.Logging;
using System.Net;
using System.Text.Json;

namespace AutoCheckAML.Api.Web.Middleware
{
    /// <summary>
    /// Middleware global para manejo de excepciones
    /// Centraliza el manejo de errores en la aplicación
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILoggerService logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ha ocurrido una excepción no manejada");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ValidationException vex:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new ErrorResponse
                    {
                        Message = vex.Message,
                        Code = vex.Code,
                        StatusCode = vex.StatusCode,
                        Errors = vex.Errors
                    };
                    break;

                case UnauthorizedException uex:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response = new ErrorResponse
                    {
                        Message = uex.Message,
                        Code = uex.Code,
                        StatusCode = uex.StatusCode
                    };
                    break;

                case NotFoundException nex:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response = new ErrorResponse
                    {
                        Message = nex.Message,
                        Code = nex.Code,
                        StatusCode = nex.StatusCode
                    };
                    break;

                case ConflictException cex:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response = new ErrorResponse
                    {
                        Message = cex.Message,
                        Code = cex.Code,
                        StatusCode = cex.StatusCode
                    };
                    break;

                case AppException aex:
                    context.Response.StatusCode = aex.StatusCode;
                    response = new ErrorResponse
                    {
                        Message = aex.Message,
                        Code = aex.Code,
                        StatusCode = aex.StatusCode
                    };
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response = new ErrorResponse
                    {
                        Message = "Ha ocurrido un error interno del servidor",
                        Code = "INTERNAL_SERVER_ERROR",
                        StatusCode = 500
                    };
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }

    /// <summary>
    /// Modelo de respuesta de error
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
