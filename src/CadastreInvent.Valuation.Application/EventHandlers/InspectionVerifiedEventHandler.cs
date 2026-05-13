using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Domain.Events;
using CadastreInvent.Valuation.Application.Handlers;
using CadastreInvent.Registry.Domain.Enums;
using CadastreInvent.Registry.Domain.Entities;

namespace CadastreInvent.Valuation.Application.EventHandlers
{
    public class InspectionVerifiedEventHandler : INotificationHandler<InspectionVerifiedEvent>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IMediator _mediator;

        public InspectionVerifiedEventHandler(CadastreDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task Handle(InspectionVerifiedEvent notification, CancellationToken cancellationToken)
        {
            var spatialUnit = await _dbContext.SpatialUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == notification.SpatialUnitId, cancellationToken);

            if (spatialUnit == null) return;

            var baUnit = await _dbContext.BAUnits
                .FirstOrDefaultAsync(b => b.SpatialUnits.Any(su => su.SpatialUnitId == notification.SpatialUnitId), cancellationToken);

            if (baUnit == null)
            {
                var newBaUnitId = Guid.NewGuid();

                baUnit = (BAUnit)FormatterServices.GetUninitializedObject(typeof(BAUnit));
                SetPropertyValue(baUnit, "Id", newBaUnitId);
                SetPropertyValue(baUnit, "Name", $"Неучтенный объект (Инспекция {notification.TaskId})");
                SetPropertyValue(baUnit, "Type", BAUnitType.BasicPropertyUnit);
                SetPropertyValue(baUnit, "CreatedAt", DateTime.UtcNow);
                SetPropertyValue(baUnit, "IsDeleted", false);

                _dbContext.BAUnits.Add(baUnit);

                var linkType = typeof(BAUnitType).Assembly.GetType("CadastreInvent.Registry.Domain.Entities.BAUnitSpatialUnit");
                if (linkType != null)
                {
                    var link = FormatterServices.GetUninitializedObject(linkType);
                    SetPropertyValue(link, "BAUnitId", newBaUnitId);
                    SetPropertyValue(link, "SpatialUnitId", notification.SpatialUnitId);
                    _dbContext.Add(link);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var valuationUnit = await _dbContext.ValuationUnits
                .FirstOrDefaultAsync(v => v.BAUnitId == baUnit.Id, cancellationToken);

            if (valuationUnit == null)
            {
                valuationUnit = new CadastreInvent.Valuation.Domain.Entities.ValuationUnit(baUnit.Id, spatialUnit.Type.ToString());
                _dbContext.ValuationUnits.Add(valuationUnit);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var characteristic = await _dbContext.PropertyCharacteristics
                .FirstOrDefaultAsync(c => c.ValuationUnitId == valuationUnit.Id, cancellationToken);

            var currentData = new CharacteristicsValidationSchema();
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (characteristic != null)
            {
                try { currentData = JsonSerializer.Deserialize<CharacteristicsValidationSchema>(characteristic.CharacteristicsJson, jsonOptions) ?? new CharacteristicsValidationSchema(); } catch { }
            }
            else
            {
                currentData.Area = (float)spatialUnit.AreaSqMeters;
                currentData.YearBuilt = (float)DateTime.UtcNow.Year;
                currentData.Floor = 1f;
                currentData.DistanceToCenterKm = 5f;
                currentData.RoomsCount = 1f;
            }

            bool isUpdated = false;

            try
            {
                var observationDataRaw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(notification.RemarksJson);
                var observationData = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
                if (observationDataRaw != null)
                {
                    foreach (var kv in observationDataRaw)
                    {
                        observationData[kv.Key] = kv.Value;
                    }

                    if (observationData.TryGetValue("notes", out var notesEl) && notesEl.ValueKind == JsonValueKind.String && notesEl.GetString()!.StartsWith("{"))
                    {
                        var innerData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(notesEl.GetString()!);
                        foreach (var kv in innerData!) observationData[kv.Key] = kv.Value;
                    }

                    if (observationData.TryGetValue("Area", out var areaEl))
                    {
                        if (areaEl.ValueKind == JsonValueKind.Number) currentData.Area = areaEl.GetSingle();
                        else if (areaEl.ValueKind == JsonValueKind.String && float.TryParse(areaEl.GetString(), out var areaVal)) currentData.Area = areaVal;
                        isUpdated = true;
                    }
                    if (observationData.TryGetValue("Floor", out var floorEl))
                    {
                        if (floorEl.ValueKind == JsonValueKind.Number) currentData.Floor = floorEl.GetSingle();
                        else if (floorEl.ValueKind == JsonValueKind.String && float.TryParse(floorEl.GetString(), out var fVal)) currentData.Floor = fVal;
                        isUpdated = true;
                    }
                    if (observationData.TryGetValue("YearBuilt", out var yearEl))
                    {
                        if (yearEl.ValueKind == JsonValueKind.Number) currentData.YearBuilt = yearEl.GetSingle();
                        else if (yearEl.ValueKind == JsonValueKind.String && float.TryParse(yearEl.GetString(), out var yVal)) currentData.YearBuilt = yVal;
                        isUpdated = true;
                    }
                    if (observationData.TryGetValue("DistanceToCenterKm", out var distEl))
                    {
                        if (distEl.ValueKind == JsonValueKind.Number) currentData.DistanceToCenterKm = distEl.GetSingle();
                        else if (distEl.ValueKind == JsonValueKind.String && float.TryParse(distEl.GetString(), out var dVal)) currentData.DistanceToCenterKm = dVal;
                        isUpdated = true;
                    }
                    if (observationData.TryGetValue("RoomsCount", out var roomsEl))
                    {
                        if (roomsEl.ValueKind == JsonValueKind.Number) currentData.RoomsCount = roomsEl.GetSingle();
                        else if (roomsEl.ValueKind == JsonValueKind.String && float.TryParse(roomsEl.GetString(), out var rVal)) currentData.RoomsCount = rVal;
                        isUpdated = true;
                    }
                }
            }
            catch { }

            if (characteristic == null)
            {
                var newJson = JsonSerializer.Serialize(currentData);
                characteristic = new CadastreInvent.Valuation.Domain.Entities.PropertyCharacteristic(valuationUnit.Id, newJson);
                characteristic.SetViolationStatus(notification.HasViolations);
                _dbContext.PropertyCharacteristics.Add(characteristic);
                isUpdated = true;
            }
            else
            {
                if (characteristic.HasViolations != notification.HasViolations) isUpdated = true;

                if (isUpdated)
                {
                    characteristic.UpdateCharacteristics(JsonSerializer.Serialize(currentData), notification.HasViolations);
                }
            }

            if (isUpdated)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _mediator.Publish(new PropertyCharacteristicsChangedEvent(
                    valuationUnit.Id,
                    notification.SpatialUnitId,
                    notification.HasViolations), cancellationToken);
            }
        }

        private void SetPropertyValue(object obj, string propertyName, object value)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) prop.SetValue(obj, value);
            else
            {
                var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) field.SetValue(obj, value);
            }
        }
    }
}