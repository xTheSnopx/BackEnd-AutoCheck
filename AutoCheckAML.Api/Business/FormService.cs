using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoCheckAML.Api.Business
{
    public interface IFormService
    {
        Task<FormSubmissionDto> CreateSubmissionAsync(int userId, CreateFormSubmissionRequest request);
        Task<PagedResult<FormSubmissionDto>> GetSubmissionsAsync(int userId, FormSubmissionFilterRequest filter);
        Task<FormSubmissionDto> GetSubmissionAsync(int userId, int formId);
        Task<bool> UpdateStatusAsync(int userId, int formId, UpdateFormSubmissionStatusRequest request);
        Task<bool> VerifySubmissionAsync(int userId, int formId, VerifyFormSubmissionRequest request);
        Task<bool> DeleteSubmissionAsync(int userId, int formId);
    }

    public class FormService : IFormService
    {
        private readonly AutoCheckAMLContext _context;

        public FormService(AutoCheckAMLContext context)
        {
            _context = context;
        }

        public async Task<FormSubmissionDto> CreateSubmissionAsync(int userId, CreateFormSubmissionRequest request)
        {
            // Validar que la plantilla exista y esté activa
            var template = await _context.FormTemplates
                .FirstOrDefaultAsync(t => t.Id == request.FormTemplateId && t.IsActive);

            if (template == null)
                throw new KeyNotFoundException($"Plantilla de formulario {request.FormTemplateId} no encontrada o inactiva.");

            var submission = new FormSubmission
            {
                FormTemplateId = request.FormTemplateId,
                SubmittedByUserId = userId,
                AssignedToCrewId = request.AssignedToCrewId,
                ActivityLocation = request.ActivityLocation,
                ActivityDate = request.ActivityDate,
                ObservationsByRespondent = request.ObservationsByRespondent,
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow
            };

            _context.FormSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Agregar respuestas
            if (request.Answers?.Any() == true)
            {
                var answers = request.Answers.Select(a => new Answer
                {
                    FormSubmissionId = submission.Id,
                    FormFieldId = a.FormFieldId,
                    FieldValue = a.FieldValue,
                    Notes = a.Notes
                }).ToList();

                _context.Answers.AddRange(answers);
                await _context.SaveChangesAsync();
            }

            return await GetSubmissionAsync(userId, submission.Id);
        }

        public async Task<PagedResult<FormSubmissionDto>> GetSubmissionsAsync(int userId, FormSubmissionFilterRequest filter)
        {
            // Obtener el rol del usuario para filtrar apropiadamente
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var query = _context.FormSubmissions
                .Include(fs => fs.FormTemplate)
                .Include(fs => fs.SubmittedByUser)
                .Include(fs => fs.AssignedToCrew)
                .Include(fs => fs.VerifiedByUser)
                .Include(fs => fs.Answers).ThenInclude(a => a.FormField)
                .AsQueryable();

            // DEV y SOFTWARE ven todos; INGENIERO solo los suyos; CUADRILLA solo los asignados
            bool isSuperUser = userRoles.Contains("DEV") || userRoles.Contains("SOFTWARE");
            bool isCuadrilla = userRoles.Contains("CUADRILLA");

            if (!isSuperUser && isCuadrilla)
            {
                var userCrewId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.CrewId)
                    .FirstOrDefaultAsync();
                query = query.Where(fs => fs.AssignedToCrewId == userCrewId);
            }
            else if (!isSuperUser)
            {
                query = query.Where(fs => fs.SubmittedByUserId == userId);
            }

            // Aplicar filtros
            if (filter.FormTemplateId.HasValue)
                query = query.Where(fs => fs.FormTemplateId == filter.FormTemplateId.Value);

            if (filter.SubmittedByUserId.HasValue)
                query = query.Where(fs => fs.SubmittedByUserId == filter.SubmittedByUserId.Value);

            if (filter.AssignedToCrewId.HasValue)
                query = query.Where(fs => fs.AssignedToCrewId == filter.AssignedToCrewId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(fs => fs.Status == filter.Status);

            if (!string.IsNullOrWhiteSpace(filter.ActivityLocation))
                query = query.Where(fs => fs.ActivityLocation.Contains(filter.ActivityLocation));

            if (filter.StartDate.HasValue)
                query = query.Where(fs => fs.ActivityDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(fs => fs.ActivityDate <= filter.EndDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(fs => fs.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<FormSubmissionDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<FormSubmissionDto> GetSubmissionAsync(int userId, int formId)
        {
            var submission = await _context.FormSubmissions
                .Include(fs => fs.FormTemplate)
                .Include(fs => fs.SubmittedByUser)
                .Include(fs => fs.AssignedToCrew)
                .Include(fs => fs.VerifiedByUser)
                .Include(fs => fs.Answers).ThenInclude(a => a.FormField)
                .FirstOrDefaultAsync(fs => fs.Id == formId);

            if (submission == null)
                throw new KeyNotFoundException("Formulario no encontrado.");

            return MapToDto(submission);
        }

        public async Task<bool> UpdateStatusAsync(int userId, int formId, UpdateFormSubmissionStatusRequest request)
        {
            var submission = await _context.FormSubmissions
                .FirstOrDefaultAsync(fs => fs.Id == formId);

            if (submission == null)
                throw new KeyNotFoundException("Formulario no encontrado.");

            submission.ChangeStatus(request.Status, userId, request.Comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifySubmissionAsync(int userId, int formId, VerifyFormSubmissionRequest request)
        {
            var submission = await _context.FormSubmissions
                .FirstOrDefaultAsync(fs => fs.Id == formId);

            if (submission == null)
                throw new KeyNotFoundException("Formulario no encontrado.");

            submission.ObservationsByRectifier = request.ObservationsByRectifier;
            submission.RequiresReview = request.RequiresReview;
            submission.VerifiedByUserId = userId;
            submission.VerifiedAt = DateTime.UtcNow;
            submission.ChangeStatus("Verificado", userId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSubmissionAsync(int userId, int formId)
        {
            var submission = await _context.FormSubmissions
                .FirstOrDefaultAsync(fs => fs.Id == formId);

            if (submission == null)
                throw new KeyNotFoundException("Formulario no encontrado.");

            _context.FormSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
            return true;
        }

        private static FormSubmissionDto MapToDto(FormSubmission fs) => new FormSubmissionDto
        {
            Id = fs.Id,
            FormTemplateId = fs.FormTemplateId,
            FormTemplateName = fs.FormTemplate?.Name,
            SubmittedByUserId = fs.SubmittedByUserId,
            SubmittedByUserName = fs.SubmittedByUser?.FullName,
            AssignedToCrewId = fs.AssignedToCrewId,
            AssignedToCrewName = fs.AssignedToCrew?.Name,
            ActivityLocation = fs.ActivityLocation,
            ActivityDate = fs.ActivityDate,
            ObservationsByRespondent = fs.ObservationsByRespondent,
            ObservationsByRectifier = fs.ObservationsByRectifier,
            VerifiedAt = fs.VerifiedAt,
            VerifiedByUserId = fs.VerifiedByUserId,
            VerifiedByUserName = fs.VerifiedByUser?.FullName,
            RequiresReview = fs.RequiresReview,
            Status = fs.Status,
            CreatedAt = fs.CreatedAt,
            Answers = fs.Answers?.Select(a => new AnswerDto
            {
                Id = a.Id,
                FormSubmissionId = a.FormSubmissionId,
                FormFieldId = a.FormFieldId,
                FormFieldLabel = a.FormField?.Label,
                FieldValue = a.FieldValue,
                Notes = a.Notes
            }).ToList() ?? new List<AnswerDto>()
        };
    }
}
