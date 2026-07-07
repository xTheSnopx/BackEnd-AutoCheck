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
    public class VehicleTypesController : ControllerBase
    {
        private readonly IFormService _formService;

        public VehicleTypesController(IFormService formService)
        {
            _formService = formService;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        [HttpGet]
        public async Task<ActionResult<List<VehicleTypeDto>>> GetAll()
        {
            try { return Ok(await _formService.GetVehicleTypesAsync()); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> Create([FromBody] CreateVehicleTypeRequest request)
        {
            try { return Ok(await _formService.CreateVehicleTypeAsync(request)); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleTypeRequest request)
        {
            try { return Ok(await _formService.UpdateVehicleTypeAsync(id, request)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _formService.DeleteVehicleTypeAsync(id);
                return Ok(new { message = "Tipo de vehículo desactivado." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}
