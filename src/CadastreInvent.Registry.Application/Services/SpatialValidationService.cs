using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Registry.Application.Services
{
    public class SpatialValidationService : ISpatialValidationService
    {
        private readonly CadastreDbContext _dbContext;
        private readonly ICoordinateTransformationService _transformService;

        public SpatialValidationService(CadastreDbContext dbContext, ICoordinateTransformationService transformService)
        {
            _dbContext = dbContext;
            _transformService = transformService;
        }

        public async Task<string> GetTopologyErrorMessageAsync(Polygon polygon, Guid? excludeId, CancellationToken cancellationToken)
        {
            var query = _dbContext.SpatialUnits.AsNoTracking();

            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            var overlapping = await query
                .Where(x => x.Boundary.Intersects(polygon))
                .ToListAsync(cancellationToken);

            var metricPolygon = _transformService.TransformFromWgs84(polygon, 28408);

            foreach (var unit in overlapping)
            {
                var metricUnit = _transformService.TransformFromWgs84(unit.Boundary, 28408);
                var intersection = metricUnit.Intersection(metricPolygon);

                if (intersection.Area > (metricPolygon.Area * 0.05))
                {
                    return $"Обнаружено существенное наложение на участок {unit.ReferenceNumber}. Площадь наложения: {intersection.Area:F1} кв.м.";
                }
            }

            return string.Empty;
        }
    }
}