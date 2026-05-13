using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record AddPropertyCharacteristicCommand(
        Guid ValuationUnitId,
        string CharacteristicsJson) : IRequest<Guid>;
}