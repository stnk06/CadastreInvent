using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CadastreInvent.Shared.Application.Reports;

namespace CadastreInvent.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportRepository _repository;
        private readonly IDocumentGeneratorService _docService;

        public ReportsController(IReportRepository repository, IDocumentGeneratorService docService)
        {
            _repository = repository;
            _docService = docService;
        }

        [HttpGet("inspection-status")]
        public async Task<IActionResult> GetInspectionStatus() => Ok(await _repository.GetInspectionStatusesAsync());

        [HttpGet("valuation-distribution")]
        public async Task<IActionResult> GetValuationDistribution() => Ok(await _repository.GetValuationDistributionAsync());

        [HttpGet("model-results")]
        public async Task<IActionResult> GetModelResults() => Ok(await _repository.GetModelResultsAsync());

        [HttpGet("dynamics")]
        public async Task<IActionResult> GetValueDynamics() => Ok(await _repository.GetValueDynamicsAsync());

        [HttpGet("data-quality")]
        public async Task<IActionResult> GetDataQuality() => Ok(await _repository.GetDataQualityAsync());

        [HttpGet("inspector-kpi")]
        public async Task<IActionResult> GetInspectorKpi() => Ok(await _repository.GetInspectorKpiAsync());

        [HttpGet("comparative")]
        public async Task<IActionResult> GetComparativeAnalysis() => Ok(await _repository.GetComparativeAnalysisAsync());

        [HttpGet("zoning-summary")]
        public async Task<IActionResult> GetZoningSummary() => Ok(await _repository.GetZoningSummaryAsync());

        [HttpGet("cadastral-extract/{baUnitId}")]
        public async Task<IActionResult> GetCadastralExtractJson(Guid baUnitId)
        {
            var data = await _repository.GetCadastralExtractAsync(baUnitId);
            if (data == null || string.IsNullOrEmpty(data.BaUnitName))
                return NotFound(new { detail = "Имущественный объект не найден или был удален." });

            return Ok(data);
        }

        [HttpGet("zoning-summary/excel")]
        public async Task<IActionResult> DownloadZoningSummaryExcel()
        {
            var data = await _repository.GetZoningSummaryAsync();
            var fileBytes = _docService.GenerateZoningSummaryExcel(data);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ZoningSummary_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }

        [HttpGet("data-quality/excel")]
        public async Task<IActionResult> DownloadDataQualityExcel()
        {
            var data = await _repository.GetDataQualityAsync();
            var fileBytes = _docService.GenerateDataQualityExcel(data);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DataQuality_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }

        [HttpGet("inspector-kpi/excel")]
        public async Task<IActionResult> DownloadInspectorKpiExcel()
        {
            var data = await _repository.GetInspectorKpiAsync();
            var fileBytes = _docService.GenerateInspectorKpiExcel(data);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"InspectorKPI_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }

        [HttpGet("comparative/excel")]
        public async Task<IActionResult> DownloadComparativeExcel()
        {
            var data = await _repository.GetComparativeAnalysisAsync();
            var fileBytes = _docService.GenerateComparativeAnalysisExcel(data);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ComparativeAnalysis_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }

        [HttpGet("cadastral-extract/{baUnitId}/pdf")]
        public async Task<IActionResult> DownloadCadastralExtractPdf(Guid baUnitId)
        {
            var data = await _repository.GetCadastralExtractAsync(baUnitId);
            if (data == null || string.IsNullOrEmpty(data.BaUnitName))
                return NotFound(new { detail = "Имущественный объект не найден или был удален." });

            byte[] mapBytes = null;
            if (!string.IsNullOrEmpty(data.MapImageUrl))
            {
                try
                {
                    // Скачиваем фрагмент карты перед отправкой в генератор PDF
                    using var client = new System.Net.Http.HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    mapBytes = await client.GetByteArrayAsync(data.MapImageUrl);
                }
                catch
                {
                    // Если интернет или API отвалился, игнорируем ошибку. PDF сформируется без карты.
                }
            }

            var fileBytes = _docService.GenerateCadastralExtractPdf(data, mapBytes);
            return File(fileBytes, "application/pdf", $"Cadastral_Passport_{baUnitId}.pdf");
        }

        [HttpGet("data-quality/pdf")]
        public async Task<IActionResult> DownloadDataQualityPdf()
        {
            var data = await _repository.GetDataQualityAsync();
            var fileBytes = _docService.GenerateDataQualityPdf(data);
            return File(fileBytes, "application/pdf", $"DataQuality_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        [HttpGet("inspector-kpi/pdf")]
        public async Task<IActionResult> DownloadInspectorKpiPdf()
        {
            var data = await _repository.GetInspectorKpiAsync();
            var fileBytes = _docService.GenerateInspectorKpiPdf(data);
            return File(fileBytes, "application/pdf", $"InspectorKPI_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        [HttpGet("comparative/pdf")]
        public async Task<IActionResult> DownloadComparativePdf()
        {
            var data = await _repository.GetComparativeAnalysisAsync();
            var fileBytes = _docService.GenerateComparativeAnalysisPdf(data);
            return File(fileBytes, "application/pdf", $"ComparativeAnalysis_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}