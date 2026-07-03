using AutoCheckAML.Api.Business;
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

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>Crear un nuevo FormSubmission respondiendo una plantilla.</summary>
        [HttpPost]
        [Authorize(Roles = "DEV,SOFTWARE,INGENIERO_MECANICO,SUPERVISOR_HSEQ,CUADRILLA")]
        public async Task<ActionResult<FormSubmissionDto>> Create([FromBody] CreateFormSubmissionRequest request)
        {
            try
            {
                var result = await _formService.CreateSubmissionAsync(GetUserId(), request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Obtener formularios con filtros paginados.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<FormSubmissionDto>>> GetAll([FromQuery] FormSubmissionFilterRequest filter)
        {
            try
            {
                var result = await _formService.GetSubmissionsAsync(GetUserId(), filter);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Obtener un formulario por ID.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FormSubmissionDto>> GetById(int id)
        {
            try
            {
                var result = await _formService.GetSubmissionAsync(GetUserId(), id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Actualizar el estado de un formulario.</summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "DEV,SOFTWARE,INGENIERO_MECANICO,SUPERVISOR_HSEQ,JEFE_MTTO")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateFormSubmissionStatusRequest request)
        {
            try
            {
                await _formService.UpdateStatusAsync(GetUserId(), id, request);
                return Ok(new { message = "Estado actualizado exitosamente." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Cuadrilla verifica el formulario.</summary>
        [HttpPut("{id}/verify")]
        [Authorize(Roles = "DEV,SOFTWARE,CUADRILLA")]
        public async Task<IActionResult> Verify(int id, [FromBody] VerifyFormSubmissionRequest request)
        {
            try
            {
                await _formService.VerifySubmissionAsync(GetUserId(), id, request);
                return Ok(new { message = "Formulario verificado exitosamente." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Eliminar un formulario (solo DEV/SOFTWARE).</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _formService.DeleteSubmissionAsync(GetUserId(), id);
                return Ok(new { message = "Formulario eliminado." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>Exportar formularios a Excel.</summary>
        [HttpGet("export/excel")]
        [Authorize(Roles = "DEV,SOFTWARE,INGENIERO_MECANICO,SUPERVISOR_HSEQ,JEFE_MTTO")]
        public async Task<IActionResult> ExportToExcel([FromQuery] FormSubmissionFilterRequest filter)
        {
            try
            {
                // Maximum export limit to prevent memory issues
                const int maxExportSize = 5000;

                var pagedResult = await _formService.GetSubmissionsAsync(GetUserId(), new FormSubmissionFilterRequest
                {
                    PageNumber = 1,
                    PageSize = maxExportSize,
                    FormTemplateId = filter.FormTemplateId,
                    Status = filter.Status,
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate
                });

                if (!pagedResult.Items.Any())
                    return NotFound(new { message = "No hay datos para exportar." });

                // Warn if there are more records than the export limit
                if (pagedResult.TotalCount > maxExportSize)
                {
                    Response.Headers.Append("X-Export-Warning", $"Solo se exportaron {maxExportSize} de {pagedResult.TotalCount} registros. Aplique filtros para reducir el conjunto de datos.");
                }

                var bytes = _exportService.ExportFormSubmissionsToExcel(pagedResult.Items);
                var fileName = $"formularios_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
