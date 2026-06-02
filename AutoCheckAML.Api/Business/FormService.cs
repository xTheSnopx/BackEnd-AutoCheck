using AutoCheckAML.Api.Data;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoCheckAML.Api.Business
{
    public interface IFormService
    {
        Task<FormSubmissionResponse> SubmitFormAsync(int userId, FormSubmissionRequest request);
        Task<List<FormSubmissionResponse>> GetAllFormSubmissionsAsync(int userId);
        Task<FormSubmissionResponse> GetFormSubmissionAsync(int userId, int formId);
        Task<List<FormSubmissionResponse>> SearchFormSubmissionsAsync(int userId, FormFilterRequest filter);
        Task<bool> UpdateFormStatusAsync(int userId, int formId, string status);
        Task<bool> DeleteFormSubmissionAsync(int userId, int formId);
    }

    public class FormService : IFormService
    {
        private readonly AutoCheckAMLContext _context;

        public FormService(AutoCheckAMLContext context)
        {
            _context = context;
        }

        public async Task<FormSubmissionResponse> SubmitFormAsync(int userId, FormSubmissionRequest request)
        {
            try
            {
                // Validaciones
                ValidateFormSubmission(request);

                var formSubmission = new FormSubmission
                {
                    UserId = userId,
                    Nombre = request.Nombre.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    Telefono = request.Telefono.Trim(),
                    Empresa = request.Empresa.Trim(),
                    Asunto = request.Asunto.Trim(),
                    Mensaje = request.Mensaje.Trim(),
                    Fecha = request.Fecha,
                    CreatedAt = DateTime.Now,
                    Status = "Pendiente"
                };

                _context.FormSubmissions.Add(formSubmission);
                await _context.SaveChangesAsync();

                return new FormSubmissionResponse
                {
                    Id = formSubmission.Id,
                    Nombre = formSubmission.Nombre,
                    Email = formSubmission.Email,
                    Telefono = formSubmission.Telefono,
                    Empresa = formSubmission.Empresa,
                    Asunto = formSubmission.Asunto,
                    Mensaje = formSubmission.Mensaje,
                    Fecha = formSubmission.Fecha,
                    CreatedAt = formSubmission.CreatedAt,
                    Status = formSubmission.Status,
                    Message = "Formulario registrado exitosamente"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar formulario: {ex.Message}");
            }
        }

        public async Task<List<FormSubmissionResponse>> GetAllFormSubmissionsAsync(int userId)
        {
            try
            {
                var forms = await _context.FormSubmissions
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                return forms.Select(f => new FormSubmissionResponse
                {
                    Id = f.Id,
                    Nombre = f.Nombre,
                    Email = f.Email,
                    Telefono = f.Telefono,
                    Empresa = f.Empresa,
                    Asunto = f.Asunto,
                    Mensaje = f.Mensaje,
                    Fecha = f.Fecha,
                    CreatedAt = f.CreatedAt,
                    Status = f.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener formularios: {ex.Message}");
            }
        }

        public async Task<FormSubmissionResponse> GetFormSubmissionAsync(int userId, int formId)
        {
            try
            {
                var form = await _context.FormSubmissions
                    .FirstOrDefaultAsync(f => f.Id == formId && f.UserId == userId);

                if (form == null)
                {
                    throw new KeyNotFoundException("Formulario no encontrado");
                }

                return new FormSubmissionResponse
                {
                    Id = form.Id,
                    Nombre = form.Nombre,
                    Email = form.Email,
                    Telefono = form.Telefono,
                    Empresa = form.Empresa,
                    Asunto = form.Asunto,
                    Mensaje = form.Mensaje,
                    Fecha = form.Fecha,
                    CreatedAt = form.CreatedAt,
                    Status = form.Status
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener formulario: {ex.Message}");
            }
        }

        public async Task<List<FormSubmissionResponse>> SearchFormSubmissionsAsync(int userId, FormFilterRequest filter)
        {
            try
            {
                var query = _context.FormSubmissions
                    .Where(f => f.UserId == userId);

                // Filtrar por término de búsqueda
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(f =>
                        f.Nombre.ToLower().Contains(searchTerm) ||
                        f.Email.ToLower().Contains(searchTerm) ||
                        f.Empresa.ToLower().Contains(searchTerm) ||
                        f.Asunto.ToLower().Contains(searchTerm));
                }

                // Filtrar por estado
                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    query = query.Where(f => f.Status == filter.Status);
                }

                // Filtrar por rango de fechas
                if (filter.StartDate.HasValue)
                {
                    query = query.Where(f => f.Fecha >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(f => f.Fecha <= filter.EndDate.Value);
                }

                // Ordenar y paginar
                var totalCount = await query.CountAsync();
                var forms = await query
                    .OrderByDescending(f => f.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return forms.Select(f => new FormSubmissionResponse
                {
                    Id = f.Id,
                    Nombre = f.Nombre,
                    Email = f.Email,
                    Telefono = f.Telefono,
                    Empresa = f.Empresa,
                    Asunto = f.Asunto,
                    Mensaje = f.Mensaje,
                    Fecha = f.Fecha,
                    CreatedAt = f.CreatedAt,
                    Status = f.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar formularios: {ex.Message}");
            }
        }

        public async Task<bool> UpdateFormStatusAsync(int userId, int formId, string status)
        {
            try
            {
                var validStatuses = new[] { "Pendiente", "Revisado", "Completado" };
                if (!validStatuses.Contains(status))
                {
                    throw new ArgumentException("Estado inválido");
                }

                var form = await _context.FormSubmissions
                    .FirstOrDefaultAsync(f => f.Id == formId && f.UserId == userId);

                if (form == null)
                {
                    throw new KeyNotFoundException("Formulario no encontrado");
                }

                form.Status = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar estado: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFormSubmissionAsync(int userId, int formId)
        {
            try
            {
                var form = await _context.FormSubmissions
                    .FirstOrDefaultAsync(f => f.Id == formId && f.UserId == userId);

                if (form == null)
                {
                    throw new KeyNotFoundException("Formulario no encontrado");
                }

                _context.FormSubmissions.Remove(form);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar formulario: {ex.Message}");
            }
        }

        private void ValidateFormSubmission(FormSubmissionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre es requerido");

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                throw new ArgumentException("El email es inválido");

            if (string.IsNullOrWhiteSpace(request.Telefono))
                throw new ArgumentException("El teléfono es requerido");

            if (string.IsNullOrWhiteSpace(request.Empresa))
                throw new ArgumentException("La empresa es requerida");

            if (string.IsNullOrWhiteSpace(request.Asunto))
                throw new ArgumentException("El asunto es requerido");

            if (string.IsNullOrWhiteSpace(request.Mensaje))
                throw new ArgumentException("El mensaje es requerido");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
