using System;
using MediatR;

namespace CadastreInvent.Shared.Application.Admin.Commands
{
    public record ChangeUserPasswordCommand(Guid UserId, string NewPassword) : IRequest<bool>;
}