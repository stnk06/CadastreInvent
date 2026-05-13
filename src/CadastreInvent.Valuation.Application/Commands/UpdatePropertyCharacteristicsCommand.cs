using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record UpdatePropertyCharacteristicsCommand(
        Guid CharacteristicId,
        string CharacteristicsJson) : IRequest<bool>;
}