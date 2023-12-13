using Microsoft.AspNetCore.Mvc;

namespace FlorianAlbert.FinanceObserver.Server.Controllers;

[ApiController]
[Route("registration")]
public class RegistrationController : ControllerBase
{
    [HttpGet("register", Name = nameof(Register))]
    public IActionResult Register()
    {
        return Ok("User 1");
    }
}