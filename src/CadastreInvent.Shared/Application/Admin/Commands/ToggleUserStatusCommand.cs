using System;
using MediatR;

namespace CadastreInvent.Shared.Application.Admin.Commands
{
    public record ToggleUserStatusCommand(Guid UserId, bool Activate) : IRequest<bool>;
}