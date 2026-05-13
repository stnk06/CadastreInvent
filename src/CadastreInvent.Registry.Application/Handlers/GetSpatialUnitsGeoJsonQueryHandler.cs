using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.Queries;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class GetSpatialUnitsGeoJsonQueryHandler : IRequestHandler<GetSpatialUnitsGeoJsonQuery, string>
    {
        private readonly CadastreDbContext _dbContext;

        public GetSpatialUnitsGeoJsonQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> Handle(GetSpatialUnitsGeoJsonQuery request, CancellationToken cancellationToken)
        {
            if (Math.Abs(request.MaxLon - request.MinLon) > 0.5 || Math.Abs(request.MaxLat - request.MinLat) > 0.5)
             {
                return CreateEmptyFeatureCollection();
            }

            var factory = new GeometryFactory(new PrecisionModel(), 4326);
            var env = new Envelope(request.MinLon, request.MaxLon, request.MinLat, request.MaxLat);
            var bbox = factory.ToGeometry(env);

            var spatialUnits = await _dbContext.SpatialUnits
                .AsNoTracking()
                .Where(x => x.Boundary.Intersects(bbox))
                .Take(1000)
                .ToListAsync(cancellationToken);

            var featureCollection = new
            {
                type = "FeatureCollection",
                features = spatialUnits.Select(su => new
                {
                    type = "Feature",
                    geometry = new
                    {
                        type = "Polygon",
                        coordinates = GetPolygonCoordinates((Polygon)su.Boundary)
                    },
                    properties = new
                    {
                        id = su.Id.ToString(),
                        referenceNumber = su.ReferenceNumber,
                        type = su.Type.ToString(),
                        areaSqMeters = su.AreaSqMeters
                    }
                })
            };

            return JsonSerializer.Serialize(featureCollection, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        private string CreateEmptyFeatureCollection()
        {
            var empty = new
            {
                type = "FeatureCollection",
                features = Array.Empty<object>()
            };
            return JsonSerializer.Serialize(empty, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        private double[][][] GetPolygonCoordinates(Polygon polygon)
        {
            var rings = new List<double[][]>
            {
                GetRingCoordinates(polygon.ExteriorRing)
            };

            for (int i = 0; i < polygon.InteriorRings.Length; i++)
            {
                rings.Add(GetRingCoordinates(polygon.InteriorRings[i]));
            }

            return rings.ToArray();
        }

        private double[][] GetRingCoordinates(LineString ring)
        {
            var coords = ring.Coordinates;
            var result = new double[coords.Length][];
            for (int i = 0; i < coords.Length; i++)
            {
                result[i] = new double[] { coords[i].X, coords[i].Y };
            }
            return result;
        }
    }
}