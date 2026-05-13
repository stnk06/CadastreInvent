using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CadastreInvent.Shared.Application.Auth;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("mobile-login")]
        public async Task<IActionResult> MobileLogin([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}