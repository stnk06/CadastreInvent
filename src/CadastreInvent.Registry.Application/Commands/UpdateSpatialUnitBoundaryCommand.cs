using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record UpdateSpatialUnitBoundaryCommand(
        Guid SpatialUnitId,
        string NewBoundaryWkt,
        double AreaSqMeters) : IRequest<bool>;
}