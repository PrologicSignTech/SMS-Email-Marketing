using Microsoft.AspNetCore.Mvc;

namespace MarketingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "Marketing Platform API",
                version = "1.0.0"
            });
        }
    }
}
