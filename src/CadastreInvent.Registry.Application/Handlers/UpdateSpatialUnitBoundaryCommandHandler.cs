using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class UpdateSpatialUnitBoundaryCommandHandler : IRequestHandler<UpdateSpatialUnitBoundaryCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public UpdateSpatialUnitBoundaryCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateSpatialUnitBoundaryCommand request, CancellationToken cancellationToken)
        {
            var spatialUnit = await _dbContext.SpatialUnits.FindAsync(new object[] { request.SpatialUnitId }, cancellationToken);
            if (spatialUnit == null) throw new Exception($"Участок с ID {request.SpatialUnitId} не найден.");

            var reader = new WKTReader();
            var polygon = (Polygon)reader.Read(request.NewBoundaryWkt);
            polygon.SRID = 4326;

            spatialUnit.UpdateBoundary(polygon, request.AreaSqMeters);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}