using System;
using MediatR;

namespace CadastreInvent.Shared.Domain.Events
{
    public record InspectionVerifiedEvent(Guid TaskId, Guid SpatialUnitId, bool HasViolations, string RemarksJson) : INotification;
}