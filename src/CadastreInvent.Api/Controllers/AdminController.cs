using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadastreInvent.Shared.Application.Admin.Commands;
using CadastreInvent.Shared.Application.Admin.Queries;

namespace CadastreInvent.Api.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _mediator.Send(new GetUsersQuery());
            return Ok(users);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _mediator.Send(new GetRolesQuery());
            return Ok(roles);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] bool activate)
        {
            await _mediator.Send(new ToggleUserStatusCommand(id, activate));
            return NoContent();
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> ChangeRole(Guid id, [FromBody] Guid newRoleId)
        {
            await _mediator.Send(new ChangeUserRoleCommand(id, newRoleId));
            return NoContent();
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var logs = await _mediator.Send(new GetAuditLogsQuery());
            return Ok(logs);
        }
    }
}