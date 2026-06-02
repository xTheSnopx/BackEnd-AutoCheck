namespace AutoCheckAML.Api.Helpers.Exceptions
{
    /// <summary>
    /// Excepción base personalizada para la aplicación
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; set; } = 500;
        public string Code { get; set; }

        public AppException(string message, string code = "APP_ERROR", int statusCode = 500) 
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Excepción para cuando un recurso no se encuentra
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) 
            : base(message, "NOT_FOUND", 404) { }
    }

    /// <summary>
    /// Excepción para validaciones fallidas
    /// </summary>
    public class ValidationException : AppException
    {
        public Dictionary<string, string[]> Errors { get; set; }

        public ValidationException(Dictionary<string, string[]> errors) 
            : base("Una o más validaciones fallaron", "VALIDATION_ERROR", 400)
        {
            Errors = errors;
        }

        public ValidationException(string message) 
            : base(message, "VALIDATION_ERROR", 400)
        {
            Errors = new Dictionary<string, string[]> 
            { 
                { "General", new[] { message } } 
            };
        }
    }

    /// <summary>
    /// Excepción para errores de autenticación
    /// </summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) 
            : base(message, "UNAUTHORIZED", 401) { }
    }

    /// <summary>
    /// Excepción para recursos ya existentes
    /// </summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message) 
            : base(message, "CONFLICT", 409) { }
    }
}
