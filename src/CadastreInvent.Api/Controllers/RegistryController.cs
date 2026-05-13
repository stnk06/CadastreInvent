using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Api.Auth;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Registry.Application.Queries;
using CadastreInvent.Shared.Application.Auth;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RegistryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly CadastreDbContext _dbContext;

        public RegistryController(IMediator mediator, CadastreDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        [HasPermission(Permissions.ManageBAUnits)]
        [HttpPost("import/parse")]
        public async Task<IActionResult> ParseExcelFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран или пуст.");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            try
            {
                var result = await _mediator.Send(new ParseExcelCommand(fileBytes, file.FileName));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { detail = ex.Message });
            }
        }

        [HasPermission(Permissions.ManageBAUnits)]
        [HttpPost("import/commit/{sessionId}")]
        public async Task<IActionResult> CommitExcelImport(Guid sessionId, [FromQuery] bool updateDuplicates = false)
        {
            try
            {
                var count = await _mediator.Send(new CommitExcelImportCommand(sessionId, updateDuplicates));
                return Ok(new { Message = $"Импорт успешно завершен. Обработано записей: {count}." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { detail = ex.Message });
            }
        }

        [HasPermission(Permissions.ManageBAUnits)]
        [HttpGet("import/history")]
        public async Task<IActionResult> GetImportHistory()
        {
            var history = await _mediator.Send(new GetImportHistoryQuery());
            return Ok(history);
        }

        // --- AJAX Lookup Endpoints for 1C-Style Dictionaries ---

        [HttpGet("lookup/baunits")]
        public async Task<IActionResult> LookupBAUnits([FromQuery] string? q)
        {
            var query = _dbContext.BAUnits.AsNoTracking().Where(b => !b.IsDeleted);
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(b => EF.Functions.ILike(b.Name, $"%{q}%"));
            }

            var results = await query.OrderByDescending(b => b.CreatedAt).Take(50)
                .Select(b => new { id = b.Id, text = b.Name })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("lookup/parties")]
        public async Task<IActionResult> LookupParties([FromQuery] string? q)
        {
            var query = _dbContext.Parties.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q}%") || EF.Functions.ILike(p.ExtId, $"%{q}%"));
            }

            var dbItems = await query.OrderByDescending(p => p.CreatedAt).Take(50)
                .Select(p => new { p.Id, p.Name, p.ExtId })
                .ToListAsync();

            var results = dbItems.Select(p => new { id = p.Id, text = $"{p.Name} (ИНН/БИН: {p.ExtId})" });
            return Ok(results);
        }

        [HttpGet("lookup/partygroups")]
        public async Task<IActionResult> LookupPartyGroups([FromQuery] string? q)
        {
            var query = _dbContext.PartyGroups.AsNoTracking().Where(g => !g.IsDeleted);
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(g => EF.Functions.ILike(g.Name, $"%{q}%"));
            }

            var results = await query.OrderByDescending(g => g.CreatedAt).Take(50)
                .Select(g => new { id = g.Id, text = g.Name })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("lookup/sources")]
        public async Task<IActionResult> LookupSources([FromQuery] string? q)
        {
            var query = _dbContext.Sources.AsNoTracking().Where(s => !s.IsDeleted);
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(s => EF.Functions.ILike(s.DocumentNumber, $"%{q}%"));
            }

            var dbItems = await query.OrderByDescending(s => s.RecordDate).Take(50)
                .Select(s => new { s.Id, s.DocumentNumber, s.RecordDate })
                .ToListAsync();

            var results = dbItems.Select(s => new {
                id = s.Id,
                text = $"№ {s.DocumentNumber} от {s.RecordDate:dd.MM.yyyy}"
            });

            return Ok(results);
        }

        [HttpGet("lookup/spatialunits")]
        public async Task<IActionResult> LookupSpatialUnits([FromQuery] string? q)
        {
            var query = _dbContext.SpatialUnits.AsNoTracking().Where(s => !s.IsDeleted);
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(s => EF.Functions.ILike(s.ReferenceNumber, $"%{q}%"));
            }

            var dbItems = await query.OrderByDescending(s => s.CreatedAt).Take(50)
                .Select(s => new { s.Id, s.ReferenceNumber })
                .ToListAsync();

            var results = dbItems.Select(s => new { id = s.Id, text = $"КН: {s.ReferenceNumber}" });
            return Ok(results);
        }

        // --- CQRS Endpoints ---

        [HasPermission(Permissions.CreateSpatialUnit)]
        [HttpPost("spatial-units")]
        public async Task<IActionResult> CreateSpatialUnit([FromBody] CreateSpatialUnitRequest request)
        {
            var command = new CreateSpatialUnitCommand(request.ReferenceNumber, request.Type, request.BoundaryWkt, request.AreaSqMeters);
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HttpGet("spatial-units/{id}")]
        public async Task<IActionResult> GetSpatialUnit(Guid id)
        {
            var result = await _mediator.Send(new GetSpatialUnitByIdQuery(id));
            return Ok(result);
        }

        [HasPermission(Permissions.UpdateBoundary)]
        [HttpPut("spatial-units/{id}/boundary")]
        public async Task<IActionResult> UpdateBoundary(Guid id, [FromBody] UpdateBoundaryRequest request)
        {
            await _mediator.Send(new UpdateSpatialUnitBoundaryCommand(id, request.NewBoundaryWkt, request.AreaSqMeters));
            return NoContent();
        }

        [HasPermission(Permissions.ManageBAUnits)]
        [HttpPost("ba-units")]
        public async Task<IActionResult> CreateBAUnit([FromBody] CreateBAUnitCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HttpGet("ba-units/{id}")]
        public async Task<IActionResult> GetBAUnit(Guid id)
        {
            var result = await _mediator.Send(new GetBAUnitByIdQuery(id));
            return Ok(result);
        }

        [HasPermission(Permissions.ManageBAUnits)]
        [HttpPost("ba-units/{id}/spatial-units")]
        public async Task<IActionResult> AddSpatialUnitToBAUnit(Guid id, [FromBody] Guid spatialUnitId)
        {
            await _mediator.Send(new AddSpatialUnitToBAUnitCommand(id, spatialUnitId));
            return NoContent();
        }

        [HasPermission(Permissions.ManageBAUnits)]
        [HttpDelete("ba-units/{baUnitId}/spatial-units/{spatialUnitId}")]
        public async Task<IActionResult> RemoveSpatialUnitFromBAUnit(Guid baUnitId, Guid spatialUnitId)
        {
            await _mediator.Send(new RemoveSpatialUnitFromBAUnitCommand(baUnitId, spatialUnitId));
            return NoContent();
        }

        [HasPermission(Permissions.ManageParties)]
        [HttpPost("parties")]
        public async Task<IActionResult> RegisterParty([FromBody] RegisterPartyCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HttpGet("parties/{id}")]
        public async Task<IActionResult> GetParty(Guid id)
        {
            var result = await _mediator.Send(new GetPartyByIdQuery(id));
            return Ok(result);
        }

        [HasPermission(Permissions.ManageParties)]
        [HttpPut("parties/{id}/contact-info")]
        public async Task<IActionResult> UpdatePartyContactInfo(Guid id, [FromBody] string newContactInfo)
        {
            await _mediator.Send(new UpdatePartyContactInfoCommand(id, newContactInfo));
            return NoContent();
        }

        [HasPermission(Permissions.ManageParties)]
        [HttpPost("party-groups")]
        public async Task<IActionResult> CreatePartyGroup([FromBody] CreatePartyGroupCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HttpGet("party-groups/{id}")]
        public async Task<IActionResult> GetPartyGroup(Guid id)
        {
            var result = await _mediator.Send(new GetPartyGroupByIdQuery(id));
            return Ok(result);
        }

        [HasPermission(Permissions.ManageParties)]
        [HttpPost("party-groups/{id}/parties")]
        public async Task<IActionResult> AddPartyToGroup(Guid id, [FromBody] AddPartyToGroupCommand command)
        {
            if (id != command.PartyGroupId) return BadRequest();
            await _mediator.Send(command);
            return NoContent();
        }

        [HasPermission(Permissions.ManageParties)]
        [HttpDelete("party-groups/{groupId}/parties/{partyId}")]
        public async Task<IActionResult> RemovePartyFromGroup(Guid groupId, Guid partyId)
        {
            await _mediator.Send(new RemovePartyFromGroupCommand(groupId, partyId));
            return NoContent();
        }

        [HasPermission(Permissions.RegisterRRR)]
        [HttpPost("rrrs")]
        public async Task<IActionResult> RegisterRRR([FromBody] RegisterRRRCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HasPermission(Permissions.TerminateRRR)]
        [HttpPut("rrrs/terminate")]
        public async Task<IActionResult> TerminateRRR([FromBody] TerminateRRRCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HasPermission(Permissions.RegisterRRR)]
        [HttpPost("sources")]
        public async Task<IActionResult> CreateSource([FromBody] CreateSourceCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }

        [HttpGet("sources/{id}")]
        public async Task<IActionResult> GetSource(Guid id)
        {
            var result = await _mediator.Send(new GetSourceByIdQuery(id));
            return Ok(result);
        }

        public record UpdateBoundaryRequest(string NewBoundaryWkt, double AreaSqMeters);
        public record CreateSpatialUnitRequest(string ReferenceNumber, SpatialUnitType Type, string BoundaryWkt, double AreaSqMeters);
    }
}