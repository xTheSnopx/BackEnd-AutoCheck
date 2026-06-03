using Microsoft.AspNetCore.Mvc;

namespace AutoCheckAML.Api.Web.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() =>
            Ok(new { status = "OK", timestamp = DateTime.UtcNow });
    }
}
