using AutoCheckAML.Api.Business;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCheckAML.Api.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<List<RoleDto>>> GetAll() =>
            Ok(await _roleService.GetAllAsync());

        [HttpGet("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<RoleDto>> GetById(int id)
        {
            try { return Ok(await _roleService.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("permissions")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<List<PermissionDto>>> GetAllPermissions() =>
            Ok(await _roleService.GetAllPermissionsAsync());

        [HttpPost("{roleId}/permissions/{permissionId}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> AssignPermission(int roleId, int permissionId)
        {
            await _roleService.AssignPermissionAsync(roleId, permissionId);
            return Ok(new { message = "Permiso asignado." });
        }

        [HttpDelete("{roleId}/permissions/{permissionId}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> RevokePermission(int roleId, int permissionId)
        {
            await _roleService.RevokePermissionAsync(roleId, permissionId);
            return Ok(new { message = "Permiso revocado." });
        }
    }
}
