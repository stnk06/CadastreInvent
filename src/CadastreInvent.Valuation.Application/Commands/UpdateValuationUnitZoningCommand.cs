using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record UpdateValuationUnitZoningCommand(
        Guid ValuationUnitId,
        string ZoningStatus) : IRequest<bool>;
}