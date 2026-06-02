using AutoCheckAML.Api.Web.DTOs;
using FluentValidation;

namespace AutoCheckAML.Api.Web.Validators
{
    // ========== AUTH ==========

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El usuario es requerido")
                .MinimumLength(3).WithMessage("Mínimo 3 caracteres")
                .MaximumLength(50);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("Mínimo 6 caracteres");
        }
    }

    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El usuario es requerido")
                .MinimumLength(3).MaximumLength(50)
                .Matches(@"^[a-zA-Z0-9_\-]+$").WithMessage("Solo alfanuméricos, guiones y guiones bajos");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("Email inválido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("Mínimo 8 caracteres")
                .Matches(@"[A-Z]").WithMessage("Debe contener al menos una mayúscula")
                .Matches(@"[0-9]").WithMessage("Debe contener al menos un número");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("El nombre completo es requerido")
                .MaximumLength(100);
        }
    }

    // ========== USERS ==========

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El usuario es requerido")
                .MinimumLength(3).MaximumLength(50)
                .Matches(@"^[a-zA-Z0-9_\-]+$").WithMessage("Solo alfanuméricos, _, -");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("Email inválido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("Mínimo 8 caracteres")
                .Matches(@"[A-Z]").WithMessage("Debe contener una mayúscula")
                .Matches(@"[0-9]").WithMessage("Debe contener un número")
                .Matches(@"[!@#$%^&*]").WithMessage("Debe contener carácter especial (!@#$%^&*)");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("El nombre completo es requerido")
                .MaximumLength(100);
        }
    }

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email inválido")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.FullName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.FullName));
        }
    }

    // ========== CREWS ==========

    public class CreateCrewRequestValidator : AbstractValidator<CreateCrewRequest>
    {
        public CreateCrewRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la cuadrilla es requerido")
                .MaximumLength(100);

            RuleFor(x => x.ManagedByUserId)
                .GreaterThan(0).WithMessage("Debe especificar un usuario responsable válido");

            RuleFor(x => x.Department)
                .MaximumLength(100);

            RuleFor(x => x.Location)
                .MaximumLength(200);
        }
    }

    // ========== FORM SUBMISSIONS ==========

    public class CreateFormSubmissionRequestValidator : AbstractValidator<CreateFormSubmissionRequest>
    {
        public CreateFormSubmissionRequestValidator()
        {
            RuleFor(x => x.FormTemplateId)
                .GreaterThan(0).WithMessage("Debe especificar una plantilla de formulario válida");

            RuleFor(x => x.ActivityLocation)
                .NotEmpty().WithMessage("La ubicación de actividad es requerida")
                .MaximumLength(200);

            RuleFor(x => x.ActivityDate)
                .NotEmpty().WithMessage("La fecha de actividad es requerida")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("La fecha de actividad no puede ser en el futuro");

            RuleFor(x => x.Answers)
                .NotNull().WithMessage("Las respuestas son requeridas");
        }
    }

    public class UpdateFormSubmissionStatusRequestValidator : AbstractValidator<UpdateFormSubmissionStatusRequest>
    {
        private static readonly string[] ValidStatuses = { "Pendiente", "En Proceso", "Verificado", "Completado", "Rechazado" };

        public UpdateFormSubmissionStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("El estado es requerido")
                .Must(s => ValidStatuses.Contains(s))
                .WithMessage($"Estado inválido. Valores válidos: {string.Join(", ", ValidStatuses)}");
        }
    }

    public class FormSubmissionFilterRequestValidator : AbstractValidator<FormSubmissionFilterRequest>
    {
        public FormSubmissionFilterRequestValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("La fecha de inicio no puede ser mayor que la de fin");
        }
    }
}
