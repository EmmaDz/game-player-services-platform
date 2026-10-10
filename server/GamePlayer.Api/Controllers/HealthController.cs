using Microsoft.AspNetCore.Mvc;

namespace GamePlayer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            service = "GamePlayer.Api",
            timestampUtc = DateTime.UtcNow
        });
    }
}