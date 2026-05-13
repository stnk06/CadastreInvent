using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadastreInvent.Shared.Application.Reports
{
    public interface IReportRepository
    {
        Task<IEnumerable<InspectionStatusStatDto>> GetInspectionStatusesAsync();
        Task<IEnumerable<ValuationDistributionDto>> GetValuationDistributionAsync();
        Task<IEnumerable<ModelResultStatDto>> GetModelResultsAsync();
        Task<IEnumerable<ZoningSummaryDto>> GetZoningSummaryAsync();
        Task<CadastralExtractDto> GetCadastralExtractAsync(Guid baUnitId);

        Task<IEnumerable<ValueDynamicsDto>> GetValueDynamicsAsync();
        Task<IEnumerable<DataQualityDto>> GetDataQualityAsync();
        Task<IEnumerable<InspectorKpiDto>> GetInspectorKpiAsync();
        Task<IEnumerable<ComparativeAnalysisDto>> GetComparativeAnalysisAsync();
    }

    public interface IDocumentGeneratorService
    {
        byte[] GenerateZoningSummaryExcel(IEnumerable<ZoningSummaryDto> data);

        byte[] GenerateCadastralExtractPdf(CadastralExtractDto extract, byte[] mapImageBytes);

        byte[] GenerateDataQualityExcel(IEnumerable<DataQualityDto> data);
        byte[] GenerateDataQualityPdf(IEnumerable<DataQualityDto> data);
        byte[] GenerateInspectorKpiExcel(IEnumerable<InspectorKpiDto> data);
        byte[] GenerateInspectorKpiPdf(IEnumerable<InspectorKpiDto> data);
        byte[] GenerateComparativeAnalysisExcel(IEnumerable<ComparativeAnalysisDto> data);
        byte[] GenerateComparativeAnalysisPdf(IEnumerable<ComparativeAnalysisDto> data);
    }

    public class InspectionStatusStatDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ValuationDistributionDto
    {
        public string RangeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public int SortOrder { get; set; }
    }

    public class ModelResultStatDto
    {
        public string ModelName { get; set; } = string.Empty;
        public string Zoning { get; set; } = string.Empty;
        public decimal AverageValue { get; set; }
    }

    public class ZoningSummaryDto
    {
        public string Zoning { get; set; } = string.Empty;
        public int ObjectsCount { get; set; }
        public double TotalArea { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class CadastralExtractDto
    {
        public Guid BaUnitId { get; set; }
        public string BaUnitName { get; set; } = string.Empty;
        public string Zoning { get; set; } = string.Empty;
        public double TotalArea { get; set; }
        public decimal AssessedValue { get; set; }
        public string Method { get; set; } = string.Empty;
        public string SpatialReferences { get; set; } = string.Empty;

        public string WktGeometries { get; set; } = string.Empty;

        public string FormattedCoordinates { get; set; } = string.Empty;
        public string MapImageUrl { get; set; } = string.Empty;
    }

    public class ValueDynamicsDto
    {
        public int Year { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class DataQualityDto
    {
        public string Zoning { get; set; } = string.Empty;
        public int TotalObjects { get; set; }
        public int MissingCoords { get; set; }
        public int MissingArea { get; set; }
        public int MissingYear { get; set; }
        public double DefectPercentage { get; set; }
    }

    public class InspectorKpiDto
    {
        public string InspectorName { get; set; } = string.Empty;
        public int AssignedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double AvgTimeHours { get; set; }
    }

    public class ComparativeAnalysisDto
    {
        public string BaUnitName { get; set; } = string.Empty;
        public decimal OldValue { get; set; }
        public decimal NewValue { get; set; }
        public decimal Difference { get; set; }
        public double DiffPercentage { get; set; }
    }
}