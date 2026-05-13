using System;
using MediatR;

namespace CadastreInvent.Shared.Application.Admin.Commands
{
    public record ChangeUserRoleCommand(Guid UserId, Guid RoleId) : IRequest<bool>;
}