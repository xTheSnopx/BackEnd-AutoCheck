using AutoCheckAML.Api.Business;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace AutoCheckAML.Api.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormTemplatesController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormTemplatesController(IFormService formService)
        {
            _formService = formService;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        // GET /api/FormTemplates - listar plantillas activas
        [HttpGet]
        public async Task<ActionResult<List<FormTemplateDto>>> GetTemplates()
        {
            try
            {
                return Ok(await _formService.GetTemplatesAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET /api/FormTemplates/active - obtener la plantilla activa global
        [HttpGet("active")]
        public async Task<ActionResult<FormTemplateDto>> GetActive()
        {
            try
            {
                return Ok(await _formService.GetActiveTemplateAsync());
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

        // PUT /api/FormTemplates/{id}/fields/bulk - guardar campos masivamente (mismo servicio que fields)
        [HttpPut("{id}/fields/bulk")]
        [Authorize(Roles = "DEV,SOFTWARE,JEFE_MTTO")]
        public async Task<IActionResult> UpdateFieldsBulk(int id, [FromBody] List<UpdateFormFieldRequest> fields)
        {
            try
            {
                await _formService.UpdateTemplateFieldsAsync(GetUserId(), id, fields);
                return Ok(new { message = "Campos del formulario actualizados con éxito." });
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

        // GET /api/FormTemplates/{id} - obtener plantilla con campos
        [HttpGet("{id}")]
        public async Task<ActionResult<FormTemplateDto>> GetTemplate(int id)
        {
            try
            {
                return Ok(await _formService.GetTemplateAsync(id));
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

        // GET /api/FormTemplates/{id}/fields - obtener campos activos ordenados
        [HttpGet("{id}/fields")]
        public async Task<ActionResult<List<FormFieldDto>>> GetFields(int id)
        {
            try
            {
                return Ok(await _formService.GetTemplateFieldsAsync(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST /api/FormTemplates - crear nueva plantilla (solo roles autorizados)
        [HttpPost]
        [Authorize(Roles = "DEV,SOFTWARE,JEFE_MTTO")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateFormTemplateRequest request)
        {
            try
            {
                var result = await _formService.CreateTemplateAsync(GetUserId(), request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT /api/FormTemplates/{id} - actualizar plantilla y campos (solo roles autorizados)
        [HttpPut("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE,JEFE_MTTO")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateFormTemplateRequest request)
        {
            try
            {
                var result = await _formService.UpdateTemplateAsync(GetUserId(), id, request);
                return Ok(result);
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

        // PUT /api/FormTemplates/{id}/fields - actualizar/crear/desactivar campos (mantengo endpoint existente)
        [HttpPut("{id}/fields")]
        [Authorize(Roles = "DEV,SOFTWARE,JEFE_MTTO")]
        public async Task<IActionResult> UpdateFields(int id, [FromBody] List<UpdateFormFieldRequest> fields)
        {
            try
            {
                await _formService.UpdateTemplateFieldsAsync(GetUserId(), id, fields);
                return Ok(new { message = "Campos del formulario actualizados con éxito." });
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
    }
}
