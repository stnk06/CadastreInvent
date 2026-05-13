using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Api.Auth;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Application.Queries;
using CadastreInvent.Valuation.Application.Services;
using CadastreInvent.Valuation.Domain.Enums;
using CadastreInvent.Shared.Application.Auth;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ValuationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMassAppraisalQueue _massAppraisalQueue;
        private readonly IMassAppraisalDiagnosticLogger _logger;
        private readonly CadastreDbContext _dbContext;

        public ValuationController(
            IMediator mediator,
            IMassAppraisalQueue massAppraisalQueue,
            IMassAppraisalDiagnosticLogger logger,
            CadastreDbContext dbContext)
        {
            _mediator = mediator;
            _massAppraisalQueue = massAppraisalQueue;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("diagnostics")]
        public IActionResult GetDiagnostics()
        {
            var logs = _logger.GetRecentLogs();
            return Ok(logs);
        }

        [HttpPost("diagnostics/clear")]
        public IActionResult ClearDiagnostics()
        {
            _logger.ClearLogs();
            return Ok();
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalUnits = await _dbContext.ValuationUnits.CountAsync(x => !EF.Property<bool>(x, "IsDeleted"));
            var validSales = await _dbContext.SalesTransactions.CountAsync(x => x.Validity == TransactionValidity.ValidMarket && !EF.Property<bool>(x, "IsDeleted"));
            var totalValuations = await _dbContext.Valuations.CountAsync(x => !EF.Property<bool>(x, "IsDeleted"));

            var latestModel = await _dbContext.MassAppraisalModels
                .AsNoTracking()
                .Where(x => !EF.Property<bool>(x, "IsDeleted"))
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                totalUnits,
                validSales,
                totalValuations,
                isReadyForTraining = validSales >= 10,
                activeModelVersion = latestModel?.Version ?? "Нет данных",
                activeModelStatus = latestModel?.Status ?? "Отсутствует",
                activeModelMetrics = latestModel?.MetricsJson ?? "{}"
            });
        }

        [HttpGet("lookup/baunits")]
        public async Task<IActionResult> LookupBAUnits([FromQuery] string? q)
        {
            var query = _dbContext.BAUnits.AsNoTracking().Where(b => !EF.Property<bool>(b, "IsDeleted"));
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(b => EF.Functions.ILike(b.Name, $"%{q}%"));
            var results = await query.OrderByDescending(b => b.CreatedAt).Take(50).Select(b => new { id = b.Id, text = b.Name }).ToListAsync();
            return Ok(results);
        }

        [HttpGet("lookup/valuationunits")]
        public async Task<IActionResult> LookupValuationUnits([FromQuery] string? q)
        {
            var query = _dbContext.ValuationUnits.AsNoTracking().Where(vu => !EF.Property<bool>(vu, "IsDeleted"));
            var baUnits = await _dbContext.BAUnits.AsNoTracking().ToDictionaryAsync(b => b.Id, b => b.Name);
            var dbItems = await query.OrderByDescending(vu => vu.CreatedAt).Take(100).ToListAsync();
            var filtered = dbItems.Select(vu => new { id = vu.Id, baUnitName = baUnits.ContainsKey(vu.BAUnitId) ? baUnits[vu.BAUnitId] : "Связь утеряна", zoning = vu.ZoningStatus });
            if (!string.IsNullOrWhiteSpace(q)) filtered = filtered.Where(x => x.baUnitName.Contains(q, StringComparison.OrdinalIgnoreCase) || x.zoning.Contains(q, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered.Take(50).Select(x => new { id = x.id, text = $"{x.baUnitName} (Зона: {x.zoning})" }));
        }

        [HttpGet("lookup/parties")]
        public async Task<IActionResult> LookupParties([FromQuery] string? q)
        {
            var query = _dbContext.Parties.AsNoTracking().Where(p => !EF.Property<bool>(p, "IsDeleted"));
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q}%") || EF.Functions.ILike(p.ExtId, $"%{q}%"));
            var dbItems = await query.OrderByDescending(p => p.CreatedAt).Take(50).Select(p => new { p.Id, p.Name, p.ExtId }).ToListAsync();
            return Ok(dbItems.Select(p => new { id = p.Id, text = $"{p.Name} (ИНН/БИН: {p.ExtId})" }));
        }

        [HttpGet("lookup/valuations")]
        public async Task<IActionResult> LookupValuations([FromQuery] string? q)
        {
            var query = _dbContext.Valuations.AsNoTracking().Where(v => !EF.Property<bool>(v, "IsDeleted"));
            var vuIds = await query.Select(v => v.ValuationUnitId).Distinct().ToListAsync();
            var vuMap = await _dbContext.ValuationUnits.Where(vu => vuIds.Contains(vu.Id)).ToDictionaryAsync(vu => vu.Id, vu => vu.BAUnitId);
            var baIds = vuMap.Values.Distinct().ToList();
            var baMap = await _dbContext.BAUnits.Where(b => baIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.Name);
            var dbItems = await query.OrderByDescending(v => v.ValuationDate).Take(100).ToListAsync();
            var mapped = dbItems.Select(v => new { id = v.Id, valDate = v.ValuationDate, price = v.AssessedValue, baUnitName = vuMap.ContainsKey(v.ValuationUnitId) && baMap.ContainsKey(vuMap[v.ValuationUnitId]) ? baMap[vuMap[v.ValuationUnitId]] : "Объект не найден" });
            if (!string.IsNullOrWhiteSpace(q)) mapped = mapped.Where(x => x.baUnitName.Contains(q, StringComparison.OrdinalIgnoreCase) || x.price.ToString().Contains(q));
            return Ok(mapped.Take(50).Select(x => new { id = x.id, text = $"{x.baUnitName} — {x.price:N0} ₽ ({x.valDate:dd.MM.yyyy})" }));
        }

        [HasPermission(Permissions.CreateValuationUnit)][HttpPost("units")] public async Task<IActionResult> CreateValuationUnit([FromBody] CreateValuationUnitCommand command) { var id = await _mediator.Send(command); return CreatedAtAction(nameof(GetValuationUnit), new { id }, new { id }); }
        [HttpGet("units/{id}")] public async Task<IActionResult> GetValuationUnit(Guid id) => Ok(await _mediator.Send(new GetValuationUnitByIdQuery(id)));
        [HasPermission(Permissions.CreateValuationUnit)][HttpPut("units/{id}/zoning")] public async Task<IActionResult> UpdateZoning(Guid id, [FromBody] string zoningStatus) { await _mediator.Send(new UpdateValuationUnitZoningCommand(id, zoningStatus)); return NoContent(); }
        [HasPermission(Permissions.CreateValuationUnit)][HttpPost("characteristics")] public async Task<IActionResult> AddCharacteristic([FromBody] AddPropertyCharacteristicCommand command) { var id = await _mediator.Send(command); return CreatedAtAction(nameof(GetCharacteristic), new { id }, new { id }); }
        [HttpGet("characteristics/{id}")] public async Task<IActionResult> GetCharacteristic(Guid id) => Ok(await _mediator.Send(new GetPropertyCharacteristicByIdQuery(id)));
        [HasPermission(Permissions.CreateValuationUnit)][HttpPut("characteristics/{id}")] public async Task<IActionResult> UpdateCharacteristic(Guid id, [FromBody] string characteristicsJson) { await _mediator.Send(new UpdatePropertyCharacteristicsCommand(id, characteristicsJson)); return NoContent(); }
        [HasPermission(Permissions.RegisterTransaction)][HttpPost("transactions")] public async Task<IActionResult> RegisterTransaction([FromBody] RegisterSalesTransactionCommand command) { var id = await _mediator.Send(command); return CreatedAtAction(nameof(GetTransaction), new { id }, new { id }); }
        [HttpGet("transactions/{id}")] public async Task<IActionResult> GetTransaction(Guid id) => Ok(await _mediator.Send(new GetSalesTransactionByIdQuery(id)));
        [HasPermission(Permissions.RegisterTransaction)][HttpPut("transactions/{id}/invalidate")] public async Task<IActionResult> InvalidateTransaction(Guid id, [FromBody] TransactionValidity newValidity) { await _mediator.Send(new InvalidateSalesTransactionCommand(id, newValidity)); return NoContent(); }
        [HttpGet("valuations/{id}")] public async Task<IActionResult> GetValuation(Guid id) => Ok(await _mediator.Send(new GetValuationByIdQuery(id)));
        [HttpGet("valuations/{id}/history")] public async Task<IActionResult> GetValuationHistory(Guid id) => Ok(await _mediator.Send(new GetValuationHistoryQuery(id)));
        [HasPermission(Permissions.ManageAppeals)][HttpPost("appeals")] public async Task<IActionResult> CreateAppeal([FromBody] CreateValuationAppealCommand command) => Ok(new { id = await _mediator.Send(command) });
        public class UpdateAppealRequest { public AppealStatus Status { get; set; } public decimal? NewAssessedValue { get; set; } }
        [HasPermission(Permissions.ManageAppeals)][HttpPut("appeals/{id}/status")] public async Task<IActionResult> UpdateAppealStatus(Guid id, [FromBody] UpdateAppealRequest request) { await _mediator.Send(new UpdateValuationAppealStatusCommand(id, request.Status, request.NewAssessedValue)); return NoContent(); }

        [HasPermission(Permissions.RegisterTransaction)]
        [HttpPost("models/train")]
        public async Task<IActionResult> TrainMassAppraisalModel([FromBody] TrainMassAppraisalModelCommand command)
        {
            _logger.LogInfo("API", "Пользователь инициировал запрос на обучение модели.");
            var id = await _mediator.Send(command);
            return Ok(new { ModelId = id });
        }

        [HasPermission(Permissions.CreateValuationUnit)]
        [HttpPost("models/execute")]
        public async Task<IActionResult> ExecuteMassAppraisal([FromBody] ExecuteMassAppraisalCommand command)
        {
            _logger.LogInfo("API", "Пользователь запустил массовую оценку (Appraisal Engine).");
            await _mediator.Send(command);
            return Accepted(new { Message = "Процесс запущен." });
        }

        // Мягкое удаление (в глобальный Архив)
        [HttpDelete("models/{id}")]
        public async Task<IActionResult> DeleteMassAppraisalModel(Guid id)
        {
            await _mediator.Send(new DeleteMassAppraisalModelCommand(id));
            _logger.LogWarning("API", $"Модель {id} скрыта в Глобальный Архив.");
            return NoContent();
        }

        [HttpGet("mass-appraisal-status")]
        public IActionResult GetMassAppraisalStatus() => Ok(new { Total = _massAppraisalQueue.GetProgress().Total, Processed = _massAppraisalQueue.GetProgress().Processed, IsRunning = _massAppraisalQueue.GetProgress().IsRunning });

        [HttpGet("list/models")]
        public async Task<IActionResult> GetModelsList([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null)
        {
            var query = _dbContext.MassAppraisalModels.AsNoTracking().Where(x => !EF.Property<bool>(x, "IsDeleted"));

            if (!string.IsNullOrEmpty(search)) query = query.Where(x => EF.Functions.ILike(x.Version, $"%{search}%") || EF.Functions.ILike(x.Algorithm, $"%{search}%"));
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new { x.Id, x.Version, x.Algorithm, x.Description, trainingDate = x.CreatedAt, status = x.Status, metrics = x.MetricsJson }).ToListAsync();
            return Ok(new { items, total });
        }

        [HttpGet("list/units")]
        public async Task<IActionResult> GetUnitsList([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? filterType = null, [FromQuery] string? search = null)
        {
            var q = from u in _dbContext.ValuationUnits.AsNoTracking().Where(u => !EF.Property<bool>(u, "IsDeleted"))
                    join b in _dbContext.BAUnits on u.BAUnitId equals b.Id
                    join c in _dbContext.PropertyCharacteristics on u.Id equals c.ValuationUnitId into cGroup
                    from c in cGroup.DefaultIfEmpty()
                    select new { Unit = u, BAUnitName = b.Name, CharId = c != null ? c.Id : (Guid?)null, CharJson = c != null ? c.CharacteristicsJson : null };
            if (!string.IsNullOrEmpty(filterType)) q = q.Where(x => x.Unit.ZoningStatus == filterType);
            if (!string.IsNullOrEmpty(search)) q = q.Where(x => EF.Functions.ILike(x.BAUnitName, $"%{search}%"));
            var total = await q.CountAsync();
            var rawItems = await q.OrderByDescending(x => x.Unit.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = rawItems.Select(x => new { id = x.Unit.Id, baUnitName = x.BAUnitName, zoningStatus = x.Unit.ZoningStatus, characteristicsId = x.CharId, characteristicsJson = x.CharJson, parsedCharacteristics = string.IsNullOrEmpty(x.CharJson) ? null : System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(x.CharJson) });
            return Ok(new { items, total });
        }

        [HttpGet("list/sales")]
        public async Task<IActionResult> GetSalesList([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? filterType = null, [FromQuery] string? search = null)
        {
            var q = from s in _dbContext.SalesTransactions.AsNoTracking().Where(s => !EF.Property<bool>(s, "IsDeleted"))
                    join u in _dbContext.ValuationUnits on s.ValuationUnitId equals u.Id
                    join b in _dbContext.BAUnits on u.BAUnitId equals b.Id
                    select new { Sale = s, BAUnitName = b.Name };
            if (!string.IsNullOrEmpty(filterType)) { if (Enum.TryParse<TransactionValidity>(filterType, out var v)) q = q.Where(x => x.Sale.Validity == v); }
            if (!string.IsNullOrEmpty(search)) q = q.Where(x => EF.Functions.ILike(x.BAUnitName, $"%{search}%"));
            var total = await q.CountAsync();
            var rawItems = await q.OrderByDescending(x => x.Sale.TransactionDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = rawItems.Select(x => new { id = x.Sale.Id, valuationUnitId = x.Sale.ValuationUnitId, baUnitName = x.BAUnitName, salePrice = x.Sale.SalePrice, transactionDate = x.Sale.TransactionDate, validity = x.Sale.Validity.ToString() });
            return Ok(new { items, total });
        }

        [HttpGet("list/results")]
        public async Task<IActionResult> GetResultsList([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? filterType = null, [FromQuery] string? search = null)
        {
            var q = from v in _dbContext.Valuations.AsNoTracking().Where(v => !EF.Property<bool>(v, "IsDeleted"))
                    join u in _dbContext.ValuationUnits on v.ValuationUnitId equals u.Id
                    join b in _dbContext.BAUnits on u.BAUnitId equals b.Id
                    join m in _dbContext.MassAppraisalModels.IgnoreQueryFilters() on v.ModelId equals m.Id into mGroup
                    from m in mGroup.DefaultIfEmpty()
                    select new { Val = v, BAUnitName = b.Name, ZoningStatus = u.ZoningStatus, ModelVersion = m != null ? m.Version : "Удаленная модель" };
            if (!string.IsNullOrEmpty(filterType)) q = q.Where(x => x.Val.Method.ToString() == filterType);
            if (!string.IsNullOrEmpty(search)) q = q.Where(x => EF.Functions.ILike(x.BAUnitName, $"%{search}%"));
            var total = await q.CountAsync();
            var rawItems = await q.OrderByDescending(x => x.Val.ValuationDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = rawItems.Select(x => new { id = x.Val.Id, baUnitName = x.BAUnitName, zoningStatus = x.ZoningStatus, assessedValue = x.Val.AssessedValue, method = x.Val.Method.ToString(), modelVersion = x.ModelVersion, valuationDate = x.Val.ValuationDate });
            return Ok(new { items, total });
        }

        [HttpGet("list/appeals")]
        public async Task<IActionResult> GetAppealsList([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? filterType = null, [FromQuery] string? search = null)
        {
            var q = from a in _dbContext.ValuationAppeals.AsNoTracking().Where(a => !EF.Property<bool>(a, "IsDeleted"))
                    join v in _dbContext.Valuations on a.ValuationId equals v.Id
                    join p in _dbContext.Parties on a.ApplicantPartyId equals p.Id
                    join u in _dbContext.ValuationUnits on v.ValuationUnitId equals u.Id
                    join b in _dbContext.BAUnits on u.BAUnitId equals b.Id
                    select new { Appeal = a, BAUnitName = b.Name, ApplicantName = p.Name, CurrentValue = v.AssessedValue };
            if (!string.IsNullOrEmpty(filterType)) { if (Enum.TryParse<AppealStatus>(filterType, out var st)) q = q.Where(x => x.Appeal.Status == st); }
            if (!string.IsNullOrEmpty(search)) q = q.Where(x => EF.Functions.ILike(x.BAUnitName, $"%{search}%") || EF.Functions.ILike(x.ApplicantName, $"%{search}%"));
            var total = await q.CountAsync();
            var rawItems = await q.OrderByDescending(x => x.Appeal.SubmissionDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = rawItems.Select(x => new { id = x.Appeal.Id, baUnitName = x.BAUnitName, applicantName = x.ApplicantName, reason = x.Appeal.Reason, currentAssessedValue = x.CurrentValue, status = x.Appeal.Status.ToString() });
            return Ok(new { items, total });
        }
    }
}