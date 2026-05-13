using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MediatR;
using NetTopologySuite.Geometries;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Api.Controllers
{
    [ApiController]
    [Route("api/gis-sync")]
    [Authorize]
    public class GisSyncController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly CadastreDbContext _dbContext;

        public GisSyncController(IMediator mediator, CadastreDbContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterSync([FromBody] SyncRegistrationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Wkt) || string.IsNullOrWhiteSpace(request.CadastralNumber))
            {
                return BadRequest(new { detail = "Необходимы WKT и Кадастровый номер." });
            }

            if (!Enum.TryParse<SpatialUnitType>(request.Type, out var spatialType))
            {
                spatialType = SpatialUnitType.Parcel;
            }

            var spatialUnitId = await _mediator.Send(new CreateSpatialUnitCommand(
                request.CadastralNumber,
                spatialType,
                request.Wkt,
                request.AreaSqMeters));

            Guid? baUnitId = null;

            if (request.CreateLinkedProperty)
            {
                var baUnitType = spatialType switch
                {
                    SpatialUnitType.Parcel => BAUnitType.BasicPropertyUnit,
                    SpatialUnitType.Building => BAUnitType.BasicPropertyUnit,
                    SpatialUnitType.Room => BAUnitType.BasicPropertyUnit,
                    SpatialUnitType.Volume3D => BAUnitType.RightOfUseUnit,
                    _ => BAUnitType.BasicPropertyUnit
                };

                baUnitId = await _mediator.Send(new CreateBAUnitCommand(
                    request.Address ?? $"Объект недвижимости ({request.CadastralNumber})",
                    baUnitType));

                await _mediator.Send(new AddSpatialUnitToBAUnitCommand(baUnitId.Value, spatialUnitId));
            }

            return Ok(new
            {
                SpatialUnitId = spatialUnitId,
                BaUnitId = baUnitId,
                Message = "Объект успешно зарегистрирован."
            });
        }

        [HttpPut("update-boundary/{id}")]
        public async Task<IActionResult> UpdateBoundary(Guid id, [FromBody] SyncUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Wkt))
            {
                return BadRequest(new { detail = "Необходимы WKT координаты." });
            }

            await _mediator.Send(new UpdateSpatialUnitBoundaryCommand(id, request.Wkt, request.AreaSqMeters));
            return Ok(new { Message = "Границы и площадь объекта успешно обновлены." });
        }

        [HttpGet("spatial-units")]
        public async Task<IActionResult> GetSpatialUnits()
        {
            var unitsWithBa = await _dbContext.SpatialUnits
                .AsNoTracking()
                .Where(u => u.Boundary != null)
                .Select(u => new
                {
                    SpatialUnit = u,
                    BaUnit = _dbContext.BAUnits.FirstOrDefault(b => b.SpatialUnits.Any(su => su.SpatialUnitId == u.Id))
                })
                .ToListAsync();

            var features = unitsWithBa
                .Where(x => x.SpatialUnit.Boundary is Polygon)
                .Select(x =>
                {
                    var polygon = (Polygon)x.SpatialUnit.Boundary;
                    var coordinates = polygon.ExteriorRing.Coordinates.Select(c => new[] { c.X, c.Y }).ToArray();

                    return new
                    {
                        type = "Feature",
                        geometry = new
                        {
                            type = "Polygon",
                            coordinates = new[] { coordinates }
                        },
                        properties = new
                        {
                            id = x.SpatialUnit.Id,
                            referenceNumber = x.SpatialUnit.ReferenceNumber,
                            type = x.SpatialUnit.Type.ToString(),
                            areaSqMeters = x.SpatialUnit.AreaSqMeters,
                            address = x.BaUnit != null ? x.BaUnit.Name : "Адрес не указан"
                        }
                    };
                });

            return Ok(new
            {
                type = "FeatureCollection",
                features = features
            });
        }
    }

    public record SyncRegistrationRequest(
        string CadastralNumber,
        string Address,
        string Wkt,
        double AreaSqMeters,
        string Type,
        bool CreateLinkedProperty);

    public record SyncUpdateRequest(
        string Wkt,
        double AreaSqMeters,
        string Type);
}