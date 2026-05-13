using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record TerminateRRRCommand(
        Guid BAUnitId,
        Guid RRRId,
        DateTime EndDate) : IRequest<bool>;
}