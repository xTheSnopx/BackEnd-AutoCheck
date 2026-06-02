namespace AutoCheckAML.Api.Helpers.Results
{
    /// <summary>
    /// Patrón Result - Manejo de resultados sin excepciones
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        // Factory methods
        public static Result<T> Success(T data, string message = "Operación exitosa")
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                Code = "SUCCESS",
                Errors = new Dictionary<string, string[]>()
            };
        }

        public static Result<T> Failure(string message, string code = "ERROR", Dictionary<string, string[]> errors = null)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = default,
                Message = message,
                Code = code,
                Errors = errors ?? new Dictionary<string, string[]>()
            };
        }
    }

    /// <summary>
    /// Resultado sin datos genéricos
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        public static Result Success(string message = "Operación exitosa")
        {
            return new Result
            {
                IsSuccess = true,
                Message = message,
                Code = "SUCCESS",
                Errors = new Dictionary<string, string[]>()
            };
        }

        public static Result Failure(string message, string code = "ERROR", Dictionary<string, string[]> errors = null)
        {
            return new Result
            {
                IsSuccess = false,
                Message = message,
                Code = code,
                Errors = errors ?? new Dictionary<string, string[]>()
            };
        }
    }
}
