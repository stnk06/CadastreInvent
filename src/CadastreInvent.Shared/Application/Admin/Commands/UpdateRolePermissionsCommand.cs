using System;
using MediatR;

namespace CadastreInvent.Shared.Application.Admin.Commands
{
    public record UpdateRolePermissionsCommand(Guid RoleId, string PermissionsJson) : IRequest<bool>;
}