using Microsoft.AspNetCore.Mvc;

namespace HealthCheckDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            return "Website1";
        }
    }
}
