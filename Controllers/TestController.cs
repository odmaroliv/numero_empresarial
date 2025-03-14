using Microsoft.AspNetCore.Mvc;

namespace NumeroEmpresarial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "API está funcionando" });
        }
    }
}
