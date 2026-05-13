using System;
using MediatR;

namespace CadastreInvent.Shared.Domain.Events
{
    public record PropertyCharacteristicsChangedEvent(
        Guid ValuationUnitId,
        Guid SpatialUnitId,
        bool HasViolations) : INotification;
}