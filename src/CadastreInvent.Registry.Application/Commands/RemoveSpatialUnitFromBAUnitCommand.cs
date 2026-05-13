using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record RemoveSpatialUnitFromBAUnitCommand(
        Guid BAUnitId,
        Guid SpatialUnitId) : IRequest<bool>;
}