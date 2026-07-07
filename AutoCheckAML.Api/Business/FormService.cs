using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

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
        Task<string> ApproveSubmissionAsync(int userId, int formId);
        Task<string> RejectSubmissionAsync(int userId, int formId, string reason);
        Task<string> SetRevisionStatusAsync(int userId, int formId, bool inRevision);

        // Template / Field management
        Task<List<FormFieldDto>> GetTemplateFieldsAsync(int templateId);
        Task<bool> UpdateTemplateFieldsAsync(int userId, int templateId, List<UpdateFormFieldRequest> fields);

        // Template lifecycle
        Task<List<FormTemplateDto>> GetTemplatesAsync();
        Task<FormTemplateDto> GetTemplateAsync(int templateId);
        Task<FormTemplateDto> GetActiveTemplateAsync();
        Task<FormTemplateDto> CreateTemplateAsync(int userId, CreateFormTemplateRequest request);
        Task<FormTemplateDto> UpdateTemplateAsync(int userId, int templateId, UpdateFormTemplateRequest request);
        Task<bool> SetActiveTemplateAsync(int templateId);

        // Vehicle Types
        Task<List<VehicleTypeDto>> GetVehicleTypesAsync();
        Task<VehicleTypeDto> CreateVehicleTypeAsync(CreateVehicleTypeRequest request);
        Task<VehicleTypeDto> UpdateVehicleTypeAsync(int id, UpdateVehicleTypeRequest request);
        Task<bool> DeleteVehicleTypeAsync(int id);
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
                VehicleTypeId = request.VehicleTypeId,
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

            // Guardar fotos/evidencias (soporta PhotoData legacy + Photos array)
            var allPhotos = new List<string>();
            if (!string.IsNullOrEmpty(request.PhotoData))
                allPhotos.Add(request.PhotoData);
            if (request.Photos?.Any() == true)
                allPhotos.AddRange(request.Photos);

            foreach (var photoBase64 in allPhotos)
            {
                try
                {
                    string base64Data = photoBase64;
                    string extension = ".jpg";
                    string contentType = "image/jpeg";
                    if (base64Data.Contains(","))
                    {
                        var parts = base64Data.Split(',');
                        var header = parts[0];
                        base64Data = parts[1];
                        if (header.Contains("image/png")) { extension = ".png"; contentType = "image/png"; }
                        else if (header.Contains("image/gif")) { extension = ".gif"; contentType = "image/gif"; }
                        else if (header.Contains("image/webp")) { extension = ".webp"; contentType = "image/webp"; }
                    }

                    byte[] bytes = Convert.FromBase64String(base64Data);
                    string fileName = $"{Guid.NewGuid()}{extension}";

                    var attachment = new Attachment
                    {
                        FormSubmissionId = submission.Id,
                        FileName = fileName,
                        ContentType = contentType,
                        FileSize = bytes.Length,
                        FilePath = $"/uploads/{fileName}",
                        FileData = bytes,
                        Description = "Evidencia fotográfica",
                        EvidenceType = "Photograph",
                        UploadedByUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Attachments.Add(attachment);
                }
                catch { /* silencioso */ }
            }
            await _context.SaveChangesAsync();

            return await GetSubmissionAsync(userId, submission.Id);
        }

        public async Task<PagedResult<FormSubmissionDto>> GetSubmissionsAsync(int userId, FormSubmissionFilterRequest filter)
        {
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
                .Include(fs => fs.VehicleType)
                .Include(fs => fs.Answers).ThenInclude(a => a.FormField)
                .Include(fs => fs.Attachments)
                .AsQueryable();

            bool isSuperUser = userRoles.Contains("DEV") || userRoles.Contains("SOFTWARE");
            bool isCuadrilla = userRoles.Contains("CUADRILLA");
            bool isApprover = userRoles.Contains("INGENIERO_MECANICO") || userRoles.Contains("SUPERVISOR_HSEQ") || userRoles.Contains("JEFE_MTTO");

            if (!isSuperUser && isCuadrilla)
            {
                var userCrewId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.CrewId)
                    .FirstOrDefaultAsync();
                query = query.Where(fs => fs.AssignedToCrewId == userCrewId);
            }
            else if (!isSuperUser && !isApprover)
            {
                query = query.Where(fs => fs.SubmittedByUserId == userId);
            }
            // INGENIERO_MECANICO, SUPERVISOR_HSEQ, JEFE_MTTO ven TODAS las inspecciones

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
                .Include(fs => fs.VehicleType)
                .Include(fs => fs.Answers).ThenInclude(a => a.FormField)
                .Include(fs => fs.Attachments)
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

        public async Task<string> ApproveSubmissionAsync(int userId, int formId)
        {
            var submission = await _context.FormSubmissions
                .FirstOrDefaultAsync(fs => fs.Id == formId);
            if (submission == null)
                throw new KeyNotFoundException("Formulario no encontrado.");

            // No se puede aprobar si está EN REVISIÓN
            if (submission.Status == "EN REVISION")
                throw new InvalidOperationException("No se puede aprobar una inspección que está EN REVISIÓN. El Ingeniero Mecánico debe quitarla de revisión primero.");

            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            bool isIngeniero = userRoles.Contains("INGENIERO_MECANICO");
            bool isSupervisor = userRoles.Contains("SUPERVISOR_HSEQ");
            bool isAdmin = userRoles.Contains("DEV") || userRoles.Contains("SOFTWARE");

            string result = "";

            if (isIngeniero || isAdmin)
            {
                if (submission.ApprovedByIngenieroId.HasValue && !isAdmin)
                    throw new InvalidOperationException("Esta inspección ya fue aprobada por un Ingeniero Mecánico.");
                submission.ApprovedByIngenieroId = userId;
                submission.ApprovedByIngenieroAt = DateTime.UtcNow;
                result = "Ingeniero Mecánico aprobó la inspección.";
            }
            else if (isSupervisor || isAdmin)
            {
                if (submission.ApprovedBySupervisorId.HasValue && !isAdmin)
                    throw new InvalidOperationException("Esta inspección ya fue aprobada por un Supervisor HSEQ.");
                submission.ApprovedBySupervisorId = userId;
                submission.ApprovedBySupervisorAt = DateTime.UtcNow;
                result = "Supervisor HSEQ aprobó la inspección.";
            }
            else
            {
                throw new InvalidOperationException("No tiene permisos para aprobar inspecciones.");
            }

            // Si ambos aprobaron (o es admin sobreescribiendo), estado = OPERATIVO
            bool ingenieroOk = submission.ApprovedByIngenieroId.HasValue;
            bool supervisorOk = submission.ApprovedBySupervisorId.HasValue;

            if (ingenieroOk && supervisorOk)
            {
                submission.Status = "OPERATIVO";
                result += " Vehículo OPERATIVO (ambas aprobaciones completas).";
            }
            else
            {
                submission.Status = "Pendiente";
                result += " Pendiente aprobación del " + (ingenieroOk ? "Supervisor HSEQ" : "Ingeniero Mecánico") + ".";
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<string> RejectSubmissionAsync(int userId, int formId, string reason)
        {
            var submission = await _context.FormSubmissions
                .FirstOrDefaultAsync(fs => fs.Id == formId);
            if (submission == null)
                throw new KeyNotFoundException("Formulario no encontrado.");

            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            bool isIngeniero = userRoles.Contains("INGENIERO_MECANICO");
            bool isSupervisor = userRoles.Contains("SUPERVISOR_HSEQ");
            bool isAdmin = userRoles.Contains("DEV") || userRoles.Contains("SOFTWARE");

            if (!isIngeniero && !isSupervisor && !isAdmin)
                throw new InvalidOperationException("No tiene permisos para rechazar inspecciones.");

            string result = "";
            if (isIngeniero || isAdmin)
            {
                result = "Ingeniero Mecánico rechazó la inspección.";
            }
            else if (isSupervisor)
            {
                result = "Supervisor HSEQ rechazó la inspección.";
            }

            // Si alguien rechaza, el vehículo queda INOPERATIVO
            submission.Status = "INOPERATIVO";
            submission.ObservationsByRectifier = string.IsNullOrEmpty(submission.ObservationsByRectifier)
                ? $"Rechazado: {reason}"
                : submission.ObservationsByRectifier + $"\nRechazado: {reason}";

            // Limpiar aprobaciones previas para que puedan revisar de nuevo
            submission.ApprovedByIngenieroId = null;
            submission.ApprovedByIngenieroAt = null;
            submission.ApprovedBySupervisorId = null;
            submission.ApprovedBySupervisorAt = null;

            await _context.SaveChangesAsync();
            return result + " Vehículo marcado como INOPERATIVO.";
        }

        public async Task<string> SetRevisionStatusAsync(int userId, int formId, bool inRevision)
        {
            var submission = await _context.FormSubmissions
                .FirstOrDefaultAsync(fs => fs.Id == formId);
            if (submission == null)
                throw new KeyNotFoundException("Formulario no encontrado.");

            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            bool isIngeniero = userRoles.Contains("INGENIERO_MECANICO");
            bool isAdmin = userRoles.Contains("DEV") || userRoles.Contains("SOFTWARE");

            // Solo el Ingeniero Mecánico puede poner/quitar EN REVISIÓN
            if (!isIngeniero && !isAdmin)
                throw new InvalidOperationException("Solo el Ingeniero Mecánico puede modificar el estado de revisión.");

            if (inRevision)
            {
                submission.Status = "EN REVISION";
                // Limpiar aprobaciones para que ambos puedan revisar de nuevo
                submission.ApprovedByIngenieroId = null;
                submission.ApprovedByIngenieroAt = null;
                submission.ApprovedBySupervisorId = null;
                submission.ApprovedBySupervisorAt = null;

                await _context.SaveChangesAsync();
                return "Vehículo puesto EN REVISIÓN. Ambos deben aprobar nuevamente.";
            }
            else
            {
                submission.Status = "Pendiente";
                await _context.SaveChangesAsync();
                return "Vehículo quitado de revisión. Ahora puede ser aprobado.";
            }
        }

        public async Task<List<FormFieldDto>> GetTemplateFieldsAsync(int templateId)
        {
            var fields = await _context.FormFields
                .Where(f => f.FormTemplateId == templateId && f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            return fields.Select(f => new FormFieldDto
            {
                Id = f.Id,
                FormTemplateId = f.FormTemplateId,
                Label = f.Label,
                Description = f.Description,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                DisplayOrder = f.DisplayOrder,
                Options = f.Options,
                ValidationRules = f.ValidationRules,
                    DefaultValue = f.DefaultValue,
                    IsActive = f.IsActive,
                    Category = f.Category,
                    CreatedAt = f.CreatedAt
            }).ToList();
        }

        public async Task<bool> UpdateTemplateFieldsAsync(int userId, int templateId, List<UpdateFormFieldRequest> fields)
        {
            var template = await _context.FormTemplates.FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null)
                throw new KeyNotFoundException($"Plantilla {templateId} no encontrada.");

            var dbFields = await _context.FormFields
                .Where(f => f.FormTemplateId == templateId)
                .ToListAsync();

            var incomingIds = fields.Where(f => f.Id > 0).Select(f => f.Id).ToList();
            var fieldsToRemove = dbFields.Where(f => !incomingIds.Contains(f.Id)).ToList();

            foreach (var fToRemove in fieldsToRemove)
            {
                var hasAnswers = await _context.Answers.AnyAsync(a => a.FormFieldId == fToRemove.Id);
                if (hasAnswers)
                {
                    fToRemove.IsActive = false;
                    fToRemove.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.FormFields.Remove(fToRemove);
                }
            }

            foreach (var fieldReq in fields)
            {
                if (fieldReq.Id > 0)
                {
                    var dbField = dbFields.FirstOrDefault(f => f.Id == fieldReq.Id);
                    if (dbField != null)
                    {
                        dbField.Label = fieldReq.Label;
                        dbField.Description = fieldReq.Description;
                        dbField.FieldType = fieldReq.FieldType;
                        dbField.IsRequired = fieldReq.IsRequired;
                        dbField.DisplayOrder = fieldReq.DisplayOrder;
                        dbField.Options = fieldReq.Options;
                        dbField.ValidationRules = fieldReq.ValidationRules;
                        dbField.DefaultValue = fieldReq.DefaultValue;
                        dbField.IsActive = fieldReq.IsActive;
                        dbField.Category = fieldReq.Category;
                        dbField.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    var newField = new FormField
                    {
                        FormTemplateId = templateId,
                        Label = fieldReq.Label,
                        Description = fieldReq.Description,
                        FieldType = fieldReq.FieldType,
                        IsRequired = fieldReq.IsRequired,
                        DisplayOrder = fieldReq.DisplayOrder,
                        Options = fieldReq.Options,
                        ValidationRules = fieldReq.ValidationRules,
                        DefaultValue = fieldReq.DefaultValue,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.FormFields.Add(newField);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<FormTemplateDto>> GetTemplatesAsync()
        {
            var templates = await _context.FormTemplates
                .Include(t => t.FormFields)
                .Include(t => t.CreatedByUser)
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            return templates.Select(t => new FormTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                FormType = t.FormType,
                Version = t.Version,
                IsActive = t.IsActive,
                RequiresSignature = t.RequiresSignature,
                DisplayOrder = t.DisplayOrder,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser?.FullName,
                CreatedAt = t.CreatedAt,
                FormFields = t.FormFields.OrderBy(f => f.DisplayOrder).Select(f => new FormFieldDto
                {
                    Id = f.Id,
                    FormTemplateId = f.FormTemplateId,
                    Label = f.Label,
                    Description = f.Description,
                    FieldType = f.FieldType,
                    IsRequired = f.IsRequired,
                    DisplayOrder = f.DisplayOrder,
                    Options = f.Options,
                    ValidationRules = f.ValidationRules,
                    DefaultValue = f.DefaultValue,
                    IsActive = f.IsActive,
                    Category = f.Category,
                    CreatedAt = f.CreatedAt
                }).ToList()
            }).ToList();
        }

        public async Task<FormTemplateDto> GetTemplateAsync(int templateId)
        {
            var t = await _context.FormTemplates
                .Include(ti => ti.FormFields)
                .Include(ti => ti.CreatedByUser)
                .FirstOrDefaultAsync(ti => ti.Id == templateId);

            if (t == null) throw new KeyNotFoundException($"Plantilla {templateId} no encontrada.");

            return new FormTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                FormType = t.FormType,
                Version = t.Version,
                IsActive = t.IsActive,
                RequiresSignature = t.RequiresSignature,
                DisplayOrder = t.DisplayOrder,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser?.FullName,
                CreatedAt = t.CreatedAt,
                FormFields = t.FormFields.OrderBy(f => f.DisplayOrder).Select(f => new FormFieldDto
                {
                    Id = f.Id,
                    FormTemplateId = f.FormTemplateId,
                    Label = f.Label,
                    Description = f.Description,
                    FieldType = f.FieldType,
                    IsRequired = f.IsRequired,
                    DisplayOrder = f.DisplayOrder,
                    Options = f.Options,
                    ValidationRules = f.ValidationRules,
                    DefaultValue = f.DefaultValue,
                    IsActive = f.IsActive,
                    Category = f.Category,
                    CreatedAt = f.CreatedAt
                }).ToList()
            };
        }

        public async Task<FormTemplateDto> GetActiveTemplateAsync()
        {
            var t = await _context.FormTemplates
                .Include(ti => ti.FormFields)
                .Include(ti => ti.CreatedByUser)
                .Where(ti => ti.IsActive)
                .OrderBy(ti => ti.DisplayOrder)
                .FirstOrDefaultAsync();

            if (t == null) throw new KeyNotFoundException("No hay una plantilla activa configurada.");

            return new FormTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                FormType = t.FormType,
                Version = t.Version,
                IsActive = t.IsActive,
                RequiresSignature = t.RequiresSignature,
                DisplayOrder = t.DisplayOrder,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser?.FullName,
                CreatedAt = t.CreatedAt,
                FormFields = t.FormFields.OrderBy(f => f.DisplayOrder).Select(f => new FormFieldDto
                {
                    Id = f.Id,
                    FormTemplateId = f.FormTemplateId,
                    Label = f.Label,
                    Description = f.Description,
                    FieldType = f.FieldType,
                    IsRequired = f.IsRequired,
                    DisplayOrder = f.DisplayOrder,
                    Options = f.Options,
                    ValidationRules = f.ValidationRules,
                    DefaultValue = f.DefaultValue,
                    IsActive = f.IsActive,
                    Category = f.Category,
                    CreatedAt = f.CreatedAt
                }).ToList()
            };
        }

        public async Task<bool> SetActiveTemplateAsync(int templateId)
        {
            var templates = await _context.FormTemplates.ToListAsync();
            foreach (var t in templates)
            {
                t.IsActive = (t.Id == templateId);
                t.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FormTemplateDto> CreateTemplateAsync(int userId, CreateFormTemplateRequest request)
        {
            var template = new FormTemplate
            {
                Name = request.Name,
                Description = request.Description,
                FormType = request.FormType,
                RequiresSignature = request.RequiresSignature,
                DisplayOrder = request.DisplayOrder,
                CreatedByUserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.FormTemplates.Add(template);
            await _context.SaveChangesAsync();

            if (request.FormFields?.Any() == true)
            {
                int order = 1;
                foreach (var f in request.FormFields)
                {
                    var newField = new FormField
                    {
                        FormTemplateId = template.Id,
                        Label = f.Label,
                        Description = f.Description,
                        FieldType = f.FieldType,
                        IsRequired = f.IsRequired,
                        DisplayOrder = f.DisplayOrder > 0 ? f.DisplayOrder : order++,
                        Options = f.Options,
                        ValidationRules = f.ValidationRules,
                        DefaultValue = f.DefaultValue,
                        Category = f.Category,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.FormFields.Add(newField);
                }
                await _context.SaveChangesAsync();
            }

            return await GetTemplateAsync(template.Id);
        }

        public async Task<FormTemplateDto> UpdateTemplateAsync(int userId, int templateId, UpdateFormTemplateRequest request)
        {
            var template = await _context.FormTemplates.FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null) throw new KeyNotFoundException($"Plantilla {templateId} no encontrada.");

            template.Name = request.Name;
            template.Description = request.Description;
            template.FormType = request.FormType;
            template.IsActive = request.IsActive;
            template.RequiresSignature = request.RequiresSignature;
            template.DisplayOrder = request.DisplayOrder;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (request.FormFields != null)
            {
                await UpdateTemplateFieldsAsync(userId, templateId, request.FormFields);
            }

            return await GetTemplateAsync(templateId);
        }

        // Vehicle Types
        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            return await _context.VehicleTypes
                .Where(v => v.IsActive)
                .OrderBy(v => v.DisplayOrder)
                .Select(v => new VehicleTypeDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Description = v.Description,
                    IsActive = v.IsActive,
                    DisplayOrder = v.DisplayOrder,
                    CreatedAt = v.CreatedAt
                }).ToListAsync();
        }

        public async Task<VehicleTypeDto> CreateVehicleTypeAsync(CreateVehicleTypeRequest request)
        {
            var vt = new VehicleType
            {
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.VehicleTypes.Add(vt);
            await _context.SaveChangesAsync();
            return new VehicleTypeDto { Id = vt.Id, Name = vt.Name, Description = vt.Description, IsActive = vt.IsActive, DisplayOrder = vt.DisplayOrder, CreatedAt = vt.CreatedAt };
        }

        public async Task<VehicleTypeDto> UpdateVehicleTypeAsync(int id, UpdateVehicleTypeRequest request)
        {
            var vt = await _context.VehicleTypes.FirstOrDefaultAsync(v => v.Id == id);
            if (vt == null) throw new KeyNotFoundException("Tipo de vehículo no encontrado.");
            vt.Name = request.Name;
            vt.Description = request.Description;
            vt.IsActive = request.IsActive;
            vt.DisplayOrder = request.DisplayOrder;
            await _context.SaveChangesAsync();
            return new VehicleTypeDto { Id = vt.Id, Name = vt.Name, Description = vt.Description, IsActive = vt.IsActive, DisplayOrder = vt.DisplayOrder, CreatedAt = vt.CreatedAt };
        }

        public async Task<bool> DeleteVehicleTypeAsync(int id)
        {
            var vt = await _context.VehicleTypes.FirstOrDefaultAsync(v => v.Id == id);
            if (vt == null) throw new KeyNotFoundException("Tipo de vehículo no encontrado.");
            vt.IsActive = false;
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
            VehicleTypeId = fs.VehicleTypeId,
            VehicleTypeName = fs.VehicleType?.Name,
            ActivityLocation = fs.ActivityLocation,
            ActivityDate = fs.ActivityDate,
            ObservationsByRespondent = fs.ObservationsByRespondent,
            ObservationsByRectifier = fs.ObservationsByRectifier,
            VerifiedAt = fs.VerifiedAt,
            VerifiedByUserId = fs.VerifiedByUserId,
            VerifiedByUserName = fs.VerifiedByUser?.FullName,
            RequiresReview = fs.RequiresReview,
            Status = fs.Status,
            ApprovedByIngenieroId = fs.ApprovedByIngenieroId,
            ApprovedByIngenieroAt = fs.ApprovedByIngenieroAt,
            ApprovedBySupervisorId = fs.ApprovedBySupervisorId,
            ApprovedBySupervisorAt = fs.ApprovedBySupervisorAt,
            CreatedAt = fs.CreatedAt,
            Answers = fs.Answers?.Select(a => new AnswerDto
            {
                Id = a.Id,
                FormSubmissionId = a.FormSubmissionId,
                FormFieldId = a.FormFieldId,
                FormFieldLabel = a.FormField?.Label,
                FieldValue = a.FieldValue,
                Notes = a.Notes
            }).ToList() ?? new List<AnswerDto>(),
            Attachments = fs.Attachments?.Select(att => new AttachmentDto
            {
                Id = att.Id,
                FormSubmissionId = att.FormSubmissionId,
                FormFieldId = att.FormFieldId,
                FormFieldLabel = att.FormField?.Label,
                FileName = att.FileName,
                ContentType = att.ContentType,
                FileSize = att.FileSize,
                FilePath = att.FilePath,
                FileDataBase64 = att.FileData != null ? Convert.ToBase64String(att.FileData) : null,
                Description = att.Description,
                EvidenceType = att.EvidenceType,
                UploadedByUserId = att.UploadedByUserId,
                UploadedByUserName = att.UploadedByUser?.FullName,
                CreatedAt = att.CreatedAt
            }).ToList() ?? new List<AttachmentDto>()
        };
    }
}
