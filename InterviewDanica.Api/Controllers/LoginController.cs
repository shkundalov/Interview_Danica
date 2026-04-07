using InterviewDanica.Api.DTOs;
using InterviewDanica.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterviewDanica.Api.Controllers;
/*
 * Token is not idempotent due to security reasons
 */
[ApiController]
[Route("api/login")]
public class LoginController : ControllerBase {
    private readonly LoginService loginService;

    public LoginController(LoginService loginService) {
        this.loginService = loginService;
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginRequestDTO request) {
        // Simple user for demonstration
        if (request.Username != "admin" || request.Password != "admin")
            return Unauthorized(new { message = "Invalid username or password" });

        var token = loginService.GenerateToken(request.Username);

        return Ok(new { token });
    }
}