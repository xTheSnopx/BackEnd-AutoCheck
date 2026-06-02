using AutoCheckAML.Api.Web.DTOs;
using FluentValidation;

namespace AutoCheckAML.Api.Web.Validators
{
    /// <summary>
    /// Validador para LoginRequest - Abstrae la lógica de validación
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El usuario es requerido")
                .MinimumLength(3).WithMessage("El usuario debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El usuario no puede exceder 50 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        }
    }

    /// <summary>
    /// Validador para RegisterRequest
    /// </summary>
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El usuario es requerido")
                .MinimumLength(3).WithMessage("El usuario debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El usuario no puede exceder 50 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email debe ser válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener una mayúscula")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener un número");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("El nombre completo es requerido")
                .MaximumLength(100).WithMessage("El nombre completo no puede exceder 100 caracteres");
        }
    }

    /// <summary>
    /// Validador para FormSubmissionRequest
    /// </summary>
    public class FormSubmissionRequestValidator : AbstractValidator<FormSubmissionRequest>
    {
        public FormSubmissionRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email debe ser válido");

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido")
                .Matches(@"^\d{7,}$").WithMessage("El teléfono debe tener al menos 7 dígitos");

            RuleFor(x => x.Empresa)
                .NotEmpty().WithMessage("La empresa es requerida")
                .MaximumLength(100).WithMessage("La empresa no puede exceder 100 caracteres");

            RuleFor(x => x.Asunto)
                .NotEmpty().WithMessage("El asunto es requerido")
                .MaximumLength(200).WithMessage("El asunto no puede exceder 200 caracteres");

            RuleFor(x => x.Mensaje)
                .NotEmpty().WithMessage("El mensaje es requerido")
                .MinimumLength(10).WithMessage("El mensaje debe tener al menos 10 caracteres")
                .MaximumLength(2000).WithMessage("El mensaje no puede exceder 2000 caracteres");

            RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha es requerida")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("La fecha no puede ser en el futuro");
        }
    }

    /// <summary>
    /// Validador para FormFilterRequest
    /// </summary>
    public class FormFilterRequestValidator : AbstractValidator<FormFilterRequest>
    {
        private static readonly string[] ValidStatuses = { "Pendiente", "Revisado", "Completado" };

        public FormFilterRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("El número de página debe ser mayor o igual a 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100");

            RuleFor(x => x.Status)
                .Must(status => string.IsNullOrEmpty(status) || ValidStatuses.Contains(status))
                .WithMessage($"El estado debe ser uno de: {string.Join(", ", ValidStatuses)}");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("La fecha de inicio no puede ser mayor que la fecha de fin");
        }
    }

    /// <summary>
    /// Validador para StatusUpdateRequest
    /// </summary>
    public class StatusUpdateRequestValidator : AbstractValidator<StatusUpdateRequest>
    {
        private static readonly string[] ValidStatuses = { "Pendiente", "Revisado", "Completado" };

        public StatusUpdateRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("El estado es requerido")
                .Must(status => ValidStatuses.Contains(status))
                .WithMessage($"El estado debe ser uno de: {string.Join(", ", ValidStatuses)}");
        }
    }
}
