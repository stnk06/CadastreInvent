using System;
using MediatR;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record StartFieldTaskCommand(Guid TaskId) : IRequest<bool>;
}