using System;
using MediatR;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record CancelFieldTaskCommand(Guid TaskId) : IRequest<bool>;
}