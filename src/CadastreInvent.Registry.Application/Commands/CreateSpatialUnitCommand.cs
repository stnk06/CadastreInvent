using System;
using MediatR;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.Commands
{
    public record CreateSpatialUnitCommand(
        string ReferenceNumber,
        SpatialUnitType Type,
        string BoundaryWkt,
        double AreaSqMeters) : IRequest<Guid>;
}