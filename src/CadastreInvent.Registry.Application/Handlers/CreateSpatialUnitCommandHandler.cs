using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class CreateSpatialUnitCommandHandler : IRequestHandler<CreateSpatialUnitCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreateSpatialUnitCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateSpatialUnitCommand request, CancellationToken cancellationToken)
        {
            var reader = new WKTReader();
            var geometry = reader.Read(request.BoundaryWkt);
            var polygon = (Polygon)geometry;
            var spatialUnit = new SpatialUnit(request.ReferenceNumber, request.Type, polygon, request.AreaSqMeters);
            _dbContext.SpatialUnits.Add(spatialUnit);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return spatialUnit.Id;
        }
    }
}