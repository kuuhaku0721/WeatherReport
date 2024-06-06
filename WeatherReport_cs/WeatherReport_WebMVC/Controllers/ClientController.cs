using Microsoft.AspNetCore.Mvc;

namespace WeatherReport_WebMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("https://localhost:7084/");
        }
    }
}
