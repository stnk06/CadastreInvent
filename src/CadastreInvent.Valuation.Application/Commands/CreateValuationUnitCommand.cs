using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record CreateValuationUnitCommand(
        Guid BAUnitId,
        string ZoningStatus) : IRequest<Guid>;
}