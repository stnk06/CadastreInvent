using System;
using MediatR;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record CreateFieldTaskCommand(
        Guid SpatialUnitId,
        Guid InspectorId,
        DateTime Deadline,
        double Longitude,
        double Latitude) : IRequest<Guid>;
}