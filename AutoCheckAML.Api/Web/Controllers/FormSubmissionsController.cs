using AutoCheckAML.Api.Business;
using AutoCheckAML.Api.Entity;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoCheckAML.Api.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormSubmissionsController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IExportService _exportService;

        public FormSubmissionsController(IFormService formService, IExportService exportService)
        {
            _formService = formService;
            _exportService = exportService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpPost("submit")]
        public async Task<ActionResult<FormSubmissionResponse>> SubmitForm([FromBody] FormSubmissionRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _formService.SubmitFormAsync(userId, request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<FormSubmissionResponse>>> GetAllForms()
        {
            try
            {
                var userId = GetUserId();
                var forms = await _formService.GetAllFormSubmissionsAsync(userId);
                return Ok(forms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FormSubmissionResponse>> GetForm(int id)
        {
            try
            {
                var userId = GetUserId();
                var form = await _formService.GetFormSubmissionAsync(userId, id);
                return Ok(form);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<List<FormSubmissionResponse>>> SearchForms([FromBody] FormFilterRequest filter)
        {
            try
            {
                var userId = GetUserId();
                var forms = await _formService.SearchFormSubmissionsAsync(userId, filter);
                return Ok(forms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateFormStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            try
            {
                var userId = GetUserId();
                var result = await _formService.UpdateFormStatusAsync(userId, id, request.Status);
                return Ok(new { message = "Estado actualizado exitosamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteForm(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _formService.DeleteFormSubmissionAsync(userId, id);
                return Ok(new { message = "Formulario eliminado exitosamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("export/excel")]
        public async Task<ActionResult> ExportToExcel()
        {
            try
            {
                var userId = GetUserId();
                var forms = await _formService.GetAllFormSubmissionsAsync(userId);

                if (!forms.Any())
                {
                    return NotFound(new { message = "No hay datos para exportar" });
                }

                // Convertir respuestas a modelos de formulario para exportar
                var data = forms.Select(f => new FormSubmission
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

                var excelBytes = _exportService.ExportToExcel(data);
                var fileName = $"formularios_{DateTime.Now:yyyy-MM-dd}.xlsx";

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    public class StatusUpdateRequest
    {
        public string Status { get; set; }
    }
}
