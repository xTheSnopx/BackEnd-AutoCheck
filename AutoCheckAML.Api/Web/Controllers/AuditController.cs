using AutoCheckAML.Api.Business;
using AutoCheckAML.Api.Web.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCheckAML.Api.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "DEV,ADMIN")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Obtiene los logs de auditoría más recientes
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<AuditLogDto>>> GetRecent([FromQuery] int limit = 100)
        {
            var logs = await _auditService.GetRecentAsync(limit);
            var dtos = logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = l.User?.Username ?? "Unknown",
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action,
                Description = l.Description,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                CreatedAt = l.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene los logs de auditoría de un usuario específico
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<AuditLogDto>>> GetByUser(int userId, [FromQuery] int limit = 50)
        {
            var logs = await _auditService.GetByUserAsync(userId, limit);
            var dtos = logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = l.User?.Username ?? "Unknown",
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action,
                Description = l.Description,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                CreatedAt = l.CreatedAt
            }).ToList();

            return Ok(dtos);
        }
    }
}
