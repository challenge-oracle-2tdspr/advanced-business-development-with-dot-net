using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroTech.Web.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("Tentativa de login para o usuário {Username}", request.Username);

            var response = await _authService.LoginAsync(request);

            if (response is null)
            {
                _logger.LogWarning("Falha de login para o usuário {Username}", request.Username);
                return Unauthorized(new { message = "Usuário ou senha inválidos." });
            }

            _logger.LogInformation("Login realizado com sucesso para o usuário {Username}", request.Username);
            return Ok(response);
        }
    }
}