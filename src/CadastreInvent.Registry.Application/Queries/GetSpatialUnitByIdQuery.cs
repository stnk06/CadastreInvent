using System;
using MediatR;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Queries
{
    public record GetSpatialUnitByIdQuery(Guid Id) : IRequest<SpatialUnitDto>;
}