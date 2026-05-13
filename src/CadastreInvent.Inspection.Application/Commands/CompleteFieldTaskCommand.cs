using System;
using MediatR;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record CompleteFieldTaskCommand(Guid TaskId) : IRequest<bool>;
}