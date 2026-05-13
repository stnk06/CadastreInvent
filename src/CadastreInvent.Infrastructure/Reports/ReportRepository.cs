using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;
using CadastreInvent.Shared.Application.Reports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace CadastreInvent.Infrastructure.Reports
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("DefaultConnection");
        }

        public async Task<IEnumerable<InspectionStatusStatDto>> GetInspectionStatusesAsync()
        {
            var sql = @"
                SELECT 
                  CASE ""State""
                    WHEN 'Created' THEN 'Создано'
                    WHEN 'Assigned' THEN 'Назначено'
                    WHEN 'InProgress' THEN 'В работе'
                    WHEN 'Completed' THEN 'Ожидает проверки'
                    WHEN 'Verified' THEN 'Утверждено'
                    WHEN 'Rejected' THEN 'Отклонено'
                    ELSE ""State""
                  END as Status,
                  COUNT(*) as Count
                FROM inspection.inspection_tasks
                WHERE ""IsDeleted"" = false
                GROUP BY ""State""
                ORDER BY Count DESC;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<InspectionStatusStatDto>(sql);
        }

        public async Task<IEnumerable<ValuationDistributionDto>> GetValuationDistributionAsync()
        {
            var sql = @"
                SELECT 
                    CASE
                        WHEN ""AssessedValue"" < 1000000 THEN 'До 1 млн ₽'
                        WHEN ""AssessedValue"" >= 1000000 AND ""AssessedValue"" < 5000000 THEN '1 - 5 млн ₽'
                        WHEN ""AssessedValue"" >= 5000000 AND ""AssessedValue"" < 15000000 THEN '5 - 15 млн ₽'
                        WHEN ""AssessedValue"" >= 15000000 AND ""AssessedValue"" < 50000000 THEN '15 - 50 млн ₽'
                        ELSE 'Свыше 50 млн ₽'
                    END as RangeName,
                    CASE
                        WHEN ""AssessedValue"" < 1000000 THEN 1
                        WHEN ""AssessedValue"" >= 1000000 AND ""AssessedValue"" < 5000000 THEN 2
                        WHEN ""AssessedValue"" >= 5000000 AND ""AssessedValue"" < 15000000 THEN 3
                        WHEN ""AssessedValue"" >= 15000000 AND ""AssessedValue"" < 50000000 THEN 4
                        ELSE 5
                    END as SortOrder,
                    COUNT(*) as Count
                FROM valuation.valuations
                WHERE ""IsDeleted"" = false
                GROUP BY RangeName, SortOrder
                ORDER BY SortOrder;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ValuationDistributionDto>(sql);
        }

        public async Task<IEnumerable<ModelResultStatDto>> GetModelResultsAsync()
        {
            var sql = @"
                SELECT 
                  m.""Version"" AS ModelName,
                  CASE u.""ZoningStatus""
                    WHEN 'Residential' THEN 'Жилая застройка'
                    WHEN 'Commercial' THEN 'Коммерческая зона'
                    WHEN 'Industrial' THEN 'Промышленная зона'
                    WHEN 'Agricultural' THEN 'Сельхозназначение'
                    ELSE u.""ZoningStatus""
                  END AS Zoning,
                  AVG(v.""AssessedValue"") AS AverageValue
                FROM valuation.valuations v
                JOIN valuation.mass_appraisal_models m ON v.""ModelId"" = m.""Id""
                JOIN valuation.valuation_units u ON v.""ValuationUnitId"" = u.""Id""
                WHERE v.""IsDeleted"" = false AND m.""IsDeleted"" = false
                GROUP BY m.""Version"", u.""ZoningStatus""
                ORDER BY m.""Version"", Zoning;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ModelResultStatDto>(sql);
        }

        public async Task<IEnumerable<ZoningSummaryDto>> GetZoningSummaryAsync()
        {
            var sql = @"
                SELECT 
                  CASE u.""ZoningStatus""
                    WHEN 'Residential' THEN 'Жилая застройка'
                    WHEN 'Commercial' THEN 'Коммерческая зона'
                    WHEN 'Industrial' THEN 'Промышленная зона'
                    WHEN 'Agricultural' THEN 'Сельхозназначение'
                    ELSE COALESCE(u.""ZoningStatus"", 'Зона не определена')
                  END AS Zoning,
                  COUNT(DISTINCT u.""Id"") AS ObjectsCount,
                  COALESCE(SUM(su.""AreaSqMeters""), 0) AS TotalArea,
                  COALESCE(SUM(v.""AssessedValue""), 0) AS TotalValue
                FROM valuation.valuation_units u
                LEFT JOIN registry.ba_unit_spatial_units bsu ON u.""BAUnitId"" = bsu.""BAUnitId""
                LEFT JOIN registry.spatial_units su ON bsu.""SpatialUnitId"" = su.""Id"" AND su.""IsDeleted"" = false
                LEFT JOIN valuation.valuations v ON u.""Id"" = v.""ValuationUnitId"" AND v.""IsDeleted"" = false
                WHERE u.""IsDeleted"" = false
                GROUP BY u.""ZoningStatus""
                ORDER BY TotalValue DESC;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ZoningSummaryDto>(sql);
        }

        public async Task<CadastralExtractDto> GetCadastralExtractAsync(Guid baUnitId)
        {
            var sql = @"
                SELECT 
                    b.""Id"" AS BaUnitId,
                    b.""Name"" AS BaUnitName,
                    CASE MAX(u.""ZoningStatus"")
                        WHEN 'Residential' THEN 'ИЖС (Жилая застройка)'
                        WHEN 'Commercial' THEN 'Коммерческая зона'
                        WHEN 'Industrial' THEN 'Промышленная зона'
                        WHEN 'Agricultural' THEN 'Сельхозназначение'
                        ELSE COALESCE(MAX(u.""ZoningStatus""), 'Не установлено')
                    END AS Zoning,
                    COALESCE(MAX(v.""AssessedValue""), 0) AS AssessedValue,
                    CASE MAX(v.""Method"")
                        WHEN 'AutomatedMachineLearning' THEN 'Автоматизированная массовая оценка'
                        WHEN 'Comparative' THEN 'Индивидуальная оценка (Оспаривание)'
                        ELSE COALESCE(MAX(v.""Method""), 'Оценка не проводилась')
                    END AS Method,
                    COALESCE(STRING_AGG(DISTINCT su.""ReferenceNumber"", ', '), 'Контуры не привязаны') AS SpatialReferences,
                    COALESCE(SUM(su.""AreaSqMeters""), 0) AS TotalArea,
                    COALESCE(STRING_AGG(DISTINCT ST_AsText(su.""Boundary""), ' | '), 'Геометрия отсутствует') AS WktGeometries
                FROM registry.ba_units b
                LEFT JOIN valuation.valuation_units u ON b.""Id"" = u.""BAUnitId"" AND u.""IsDeleted"" = false
                LEFT JOIN valuation.valuations v ON u.""Id"" = v.""ValuationUnitId"" AND v.""IsDeleted"" = false
                LEFT JOIN registry.ba_unit_spatial_units bsu ON b.""Id"" = bsu.""BAUnitId""
                LEFT JOIN registry.spatial_units su ON bsu.""SpatialUnitId"" = su.""Id"" AND su.""IsDeleted"" = false
                WHERE b.""Id"" = @BaUnitId AND b.""IsDeleted"" = false
                GROUP BY b.""Id"", b.""Name"";";

            using var connection = new NpgsqlConnection(_connectionString);
            var extract = await connection.QueryFirstOrDefaultAsync<CadastralExtractDto>(sql, new { BaUnitId = baUnitId });

            if (extract != null)
            {
                FormatWktAndGenerateMapUrl(extract);
            }

            return extract;
        }

        private void FormatWktAndGenerateMapUrl(CadastralExtractDto extract)
        {
            if (string.IsNullOrWhiteSpace(extract.WktGeometries) || extract.WktGeometries == "Геометрия отсутствует")
            {
                extract.FormattedCoordinates = "Пространственные границы объекта не установлены.";
                return;
            }

            var polygons = extract.WktGeometries.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            var mapPoints = new List<string>();

            for (int i = 0; i < polygons.Length; i++)
            {
                sb.AppendLine($"--- ПРОСТРАНСТВЕННЫЙ КОНТУР {i + 1} ---");
                var matches = Regex.Matches(polygons[i], @"(-?\d+\.?\d*)\s+(-?\d+\.?\d*)");
                int ptIdx = 1;

                foreach (Match m in matches)
                {
                    string lon = m.Groups[1].Value.Replace(",", ".");
                    string lat = m.Groups[2].Value.Replace(",", ".");

                    sb.AppendLine($"Точка {ptIdx,-3} : Широта {lat}° с.ш., Долгота {lon}° в.д.");

                    if (mapPoints.Count < 60)
                    {
                        mapPoints.Add($"{lon},{lat}");
                    }
                    ptIdx++;
                }
                sb.AppendLine();
            }

            extract.FormattedCoordinates = sb.ToString().TrimEnd();

            if (mapPoints.Count > 0)
            {
                string polyParams = string.Join(",", mapPoints);
                extract.MapImageUrl = $"https://static-maps.yandex.ru/1.x/?l=map&pl=c:1d4ed8FF,f:3b82f640,w:2,{polyParams}";
            }
        }

        public async Task<IEnumerable<ValueDynamicsDto>> GetValueDynamicsAsync()
        {
            var sql = @"
                SELECT 
                  EXTRACT(YEAR FROM ""ValuationDate"")::int AS Year,
                  SUM(""AssessedValue"") AS TotalValue
                FROM valuation.valuations
                WHERE ""IsDeleted"" = false
                GROUP BY EXTRACT(YEAR FROM ""ValuationDate"")
                ORDER BY Year ASC;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ValueDynamicsDto>(sql);
        }

        public async Task<IEnumerable<DataQualityDto>> GetDataQualityAsync()
        {
            var sql = @"
                SELECT
                  CASE u.""ZoningStatus""
                    WHEN 'Residential' THEN 'Жилая застройка'
                    WHEN 'Commercial' THEN 'Коммерческая зона'
                    WHEN 'Industrial' THEN 'Промышленная зона'
                    WHEN 'Agricultural' THEN 'Сельхозназначение'
                    ELSE COALESCE(u.""ZoningStatus"", 'Не указано')
                  END AS Zoning,
                  COUNT(u.""Id"") AS TotalObjects,
                  SUM(CASE WHEN su.""Boundary"" IS NULL THEN 1 ELSE 0 END) AS MissingCoords,
                  SUM(CASE WHEN NULLIF(pc.""CharacteristicsJson""->>'Area', '') IS NULL OR pc.""CharacteristicsJson""->>'Area' = '0' THEN 1 ELSE 0 END) AS MissingArea,
                  SUM(CASE WHEN NULLIF(pc.""CharacteristicsJson""->>'YearBuilt', '') IS NULL OR pc.""CharacteristicsJson""->>'YearBuilt' = '0' THEN 1 ELSE 0 END) AS MissingYear
                FROM valuation.valuation_units u
                LEFT JOIN registry.ba_unit_spatial_units bsu ON u.""BAUnitId"" = bsu.""BAUnitId""
                LEFT JOIN registry.spatial_units su ON bsu.""SpatialUnitId"" = su.""Id"" AND su.""IsDeleted"" = false
                LEFT JOIN valuation.property_characteristics pc ON u.""Id"" = pc.""ValuationUnitId"" AND pc.""IsDeleted"" = false
                WHERE u.""IsDeleted"" = false
                GROUP BY u.""ZoningStatus"";";

            using var connection = new NpgsqlConnection(_connectionString);
            var results = await connection.QueryAsync<DataQualityDto>(sql);

            foreach (var item in results)
            {
                if (item.TotalObjects > 0)
                {
                    double defects = item.MissingCoords + item.MissingArea + item.MissingYear;
                    item.DefectPercentage = Math.Round((defects / (item.TotalObjects * 3.0)) * 100, 1);
                }
            }

            return results.OrderByDescending(r => r.DefectPercentage);
        }

        public async Task<IEnumerable<InspectorKpiDto>> GetInspectorKpiAsync()
        {
            var sql = @"
                SELECT
                  u.""Username"" AS InspectorName,
                  COUNT(t.""Id"") AS AssignedTasks,
                  SUM(CASE WHEN t.""State"" IN ('Completed', 'Verified') THEN 1 ELSE 0 END) AS CompletedTasks,
                  SUM(CASE WHEN t.""Deadline"" < now() AND t.""State"" NOT IN ('Completed', 'Verified', 'Rejected', 'Cancelled') THEN 1 ELSE 0 END) AS OverdueTasks,
                  COALESCE(AVG(EXTRACT(EPOCH FROM (t.""UpdatedAt"" - t.""CreatedAt"")) / 3600.0) FILTER (WHERE t.""State"" IN ('Completed', 'Verified')), 0) AS AvgTimeHours
                FROM shared.users u
                JOIN inspection.inspection_tasks t ON t.""AssignedInspectorId"" = u.""Id""
                WHERE t.""IsDeleted"" = false
                GROUP BY u.""Username""
                ORDER BY CompletedTasks DESC;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<InspectorKpiDto>(sql);
        }

        public async Task<IEnumerable<ComparativeAnalysisDto>> GetComparativeAnalysisAsync()
        {
            // ИСПРАВЛЕНО: Заменен ABS(Difference) на математическое выражение в ORDER BY
            var sql = @"
                SELECT
                    b.""Name"" AS BaUnitName,
                    COALESCE(vh.""AssessedValue"", 0) AS OldValue,
                    v.""AssessedValue"" AS NewValue,
                    (v.""AssessedValue"" - COALESCE(vh.""AssessedValue"", 0)) AS Difference,
                    CASE WHEN COALESCE(vh.""AssessedValue"", 0) > 0 THEN ((v.""AssessedValue"" - vh.""AssessedValue"") / vh.""AssessedValue"") * 100 ELSE 0 END AS DiffPercentage
                FROM valuation.valuations v
                JOIN valuation.valuation_units vu ON v.""ValuationUnitId"" = vu.""Id""
                JOIN registry.ba_units b ON vu.""BAUnitId"" = b.""Id""
                LEFT JOIN LATERAL (
                    SELECT ""AssessedValue"" FROM valuation.valuations_history v_hist
                    WHERE v_hist.""Id"" = v.""Id"" AND v_hist.""ValidTo"" <= v.""ValidFrom""
                    ORDER BY ""ValidTo"" DESC LIMIT 1
                ) vh ON true
                WHERE v.""IsDeleted"" = false
                  AND COALESCE(vh.""AssessedValue"", 0) > 0
                  AND ABS(((v.""AssessedValue"" - vh.""AssessedValue"") / vh.""AssessedValue"") * 100) > 30
                ORDER BY ABS(v.""AssessedValue"" - COALESCE(vh.""AssessedValue"", 0)) DESC LIMIT 100;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ComparativeAnalysisDto>(sql);
        }
    }
}