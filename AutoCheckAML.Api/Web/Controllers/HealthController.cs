using Microsoft.AspNetCore.Mvc;
using AutoCheckAML.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AutoCheckAML.Api.Web.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AutoCheckAMLContext _context;

        public HealthController(AutoCheckAMLContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var templates = await _context.FormTemplates
                .Select(t => new { t.Id, t.Name, t.IsActive })
                .ToListAsync();
            return Ok(new { status = "OK", timestamp = DateTime.UtcNow, templates });
        }
    }
}
