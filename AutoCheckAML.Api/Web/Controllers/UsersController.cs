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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IExportService _exportService;

        public UsersController(IUserService userService, IExportService exportService)
        {
            _userService = userService;
            _exportService = exportService;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        [HttpGet]
        [Authorize(Roles = "DEV,SOFTWARE,JEFE_MTTO")]
        public async Task<ActionResult<List<UserDto>>> GetAll() =>
            Ok(await _userService.GetAllAsync());

        [HttpGet("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            try { return Ok(await _userService.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            try { return Ok(await _userService.GetByIdAsync(GetUserId())); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest request)
        {
            try { return Ok(await _userService.UpdateAsync(id, request)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DEV")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                return Ok(new { message = "Usuario eliminado." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("{id}/roles/{roleId}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> AssignRole(int id, int roleId)
        {
            await _userService.AssignRoleAsync(id, roleId);
            return Ok(new { message = "Rol asignado." });
        }

        [HttpDelete("{id}/roles/{roleId}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> RevokeRole(int id, int roleId)
        {
            await _userService.RevokeRoleAsync(id, roleId);
            return Ok(new { message = "Rol revocado." });
        }

        [HttpPost("{id}/change-password")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _userService.ChangePasswordAsync(id, request.NewPassword);
                return Ok(new { message = "Contraseña actualizada exitosamente." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpGet("export/excel")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> ExportToExcel()
        {
            var users = await _userService.GetAllAsync();
            if (!users.Any()) return NotFound(new { message = "No hay usuarios para exportar." });
            var bytes = _exportService.ExportUsersToExcel(users);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"usuarios_{DateTime.Now:yyyy-MM-dd}.xlsx");
        }
    }
}
