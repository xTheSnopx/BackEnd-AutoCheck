using FluentValidation;
using AppValidationException = AutoCheckAML.Api.Helpers.Exceptions.ValidationException;

namespace AutoCheckAML.Api.Web.Middleware
{
    /// <summary>
    /// Extensión de validación usando FluentValidation
    /// Valida los DTOs antes de pasar a los servicios
    /// </summary>
    public static class ValidationExtensions
    {
        public static async Task ValidateAsync<T>(this T model, IValidator<T> validator) 
            where T : class
        {
            var validationResult = await validator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );

                throw new AppValidationException(errors);
            }
        }
    }
}
