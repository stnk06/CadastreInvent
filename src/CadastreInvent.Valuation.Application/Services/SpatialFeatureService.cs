using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Services
{
    public class SpatialMetrics
    {
        public double ActualAreaSqMeters { get; set; }
        public double DistanceToCenterKm { get; set; }
    }

    public interface ISpatialFeatureService
    {
        Task<Dictionary<Guid, SpatialMetrics>> GetSpatialMetricsAsync(IEnumerable<Guid> valuationUnitIds, CancellationToken cancellationToken);
    }

    public class SpatialFeatureService : ISpatialFeatureService
    {
        private readonly CadastreDbContext _dbContext;

        public SpatialFeatureService(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<Guid, SpatialMetrics>> GetSpatialMetricsAsync(IEnumerable<Guid> valuationUnitIds, CancellationToken cancellationToken)
        {
            var idList = valuationUnitIds.ToList();
            if (!idList.Any()) return new Dictionary<Guid, SpatialMetrics>();

            var sql = @"
                SELECT
                    vu.""Id"" AS ValuationUnitId,
                    SUM(ST_Area(su.""Boundary""::geography)) AS ActualAreaSqMeters,
                    MIN(ST_Distance(su.""Boundary""::geography, ST_GeomFromText('POINT(37.6173 55.7558)', 4326)::geography)) / 1000.0 AS DistanceToCenterKm
                FROM valuation.valuation_units vu
                JOIN registry.ba_unit_spatial_units bsu ON bsu.""BAUnitId"" = vu.""BAUnitId""
                JOIN registry.spatial_units su ON su.""Id"" = bsu.""SpatialUnitId""
                WHERE vu.""Id"" = ANY(@p0) AND su.""IsDeleted"" = false
                GROUP BY vu.""Id""
            ";

            var result = new Dictionary<Guid, SpatialMetrics>();

            using var command = _dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            var param = command.CreateParameter();
            param.ParameterName = "@p0";
            param.Value = idList.ToArray();
            command.Parameters.Add(param);

            bool wasClosed = command.Connection.State == ConnectionState.Closed;
            if (wasClosed) await command.Connection.OpenAsync(cancellationToken);

            try
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var vuId = reader.GetGuid(0);
                    if (!result.ContainsKey(vuId))
                    {
                        result.Add(vuId, new SpatialMetrics
                        {
                            ActualAreaSqMeters = reader.GetDouble(1),
                            DistanceToCenterKm = reader.GetDouble(2)
                        });
                    }
                }
            }
            finally
            {
                if (wasClosed) await command.Connection.CloseAsync();
            }

            return result;
        }
    }
}