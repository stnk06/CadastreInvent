using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record AddSpatialUnitToBAUnitCommand(
        Guid BAUnitId,
        Guid SpatialUnitId) : IRequest<bool>;
}