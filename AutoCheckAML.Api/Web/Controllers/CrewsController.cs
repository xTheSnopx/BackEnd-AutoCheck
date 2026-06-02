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
    public class CrewsController : ControllerBase
    {
        private readonly ICrewService _crewService;

        public CrewsController(ICrewService crewService)
        {
            _crewService = crewService;
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        [HttpGet]
        public async Task<ActionResult<List<CrewDto>>> GetAll() =>
            Ok(await _crewService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<CrewDto>> GetById(int id)
        {
            try { return Ok(await _crewService.GetByIdAsync(id)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<CrewDto>> Create([FromBody] CreateCrewRequest request)
        {
            try
            {
                var crew = await _crewService.CreateAsync(GetUserId(), request);
                return CreatedAtAction(nameof(GetById), new { id = crew.Id }, crew);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<ActionResult<CrewDto>> Update(int id, [FromBody] UpdateCrewRequest request)
        {
            try { return Ok(await _crewService.UpdateAsync(id, request)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _crewService.DeleteAsync(id);
                return Ok(new { message = "Cuadrilla desactivada." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpGet("{id}/members")]
        public async Task<ActionResult<List<UserDto>>> GetMembers(int id) =>
            Ok(await _crewService.GetMembersAsync(id));

        [HttpPost("{id}/members/{userId}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> AddMember(int id, int userId)
        {
            try
            {
                await _crewService.AddMemberAsync(id, userId);
                return Ok(new { message = "Miembro agregado." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id}/members/{userId}")]
        [Authorize(Roles = "DEV,SOFTWARE")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        {
            await _crewService.RemoveMemberAsync(id, userId);
            return Ok(new { message = "Miembro removido." });
        }
    }
}
