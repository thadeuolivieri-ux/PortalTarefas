using Microsoft.AspNetCore.Mvc;

namespace PortalTarefas.Web.Controllers;

[ApiController]
[Route("api/mvc/[controller]")]
public class DiagnosticoController : ControllerBase
{
    [HttpGet]
    public IActionResult GetDiagnostico()
    {
        var info = new
        {
            Origem = "Controller MVC",
            HorarioUtc = DateTime.UtcNow,
            VersaoRuntime = Environment.Version.ToString(),
            Status = "Saudável"
        };

        return Ok(info);
    }
}
