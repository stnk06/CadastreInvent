using System;
using MediatR;

namespace CadastreInvent.Shared.Application.Admin.Commands
{
    public record CreateUserCommand(string Username, string Email, string Password, Guid RoleId) : IRequest<Guid>;
}