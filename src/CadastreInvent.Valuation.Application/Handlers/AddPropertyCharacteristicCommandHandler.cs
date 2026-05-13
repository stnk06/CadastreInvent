using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class AddPropertyCharacteristicCommandHandler : IRequestHandler<AddPropertyCharacteristicCommand, Guid>
    {
        private readonly CadastreDbContext _context;

        public AddPropertyCharacteristicCommandHandler(CadastreDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(AddPropertyCharacteristicCommand request, CancellationToken cancellationToken)
        {
            var unitExists = await _context.ValuationUnits
                .AnyAsync(u => u.Id == request.ValuationUnitId, cancellationToken);

            if (!unitExists)
            {
                throw new ArgumentException($"Объект оценки с Id {request.ValuationUnitId} не найден.");
            }

            var existingCharacteristic = await _context.PropertyCharacteristics
                .FirstOrDefaultAsync(c => c.ValuationUnitId == request.ValuationUnitId, cancellationToken);

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            CharacteristicsValidationSchema parsedData;

            try
            {
                parsedData = JsonSerializer.Deserialize<CharacteristicsValidationSchema>(request.CharacteristicsJson, jsonOptions);
                if (parsedData == null || !parsedData.IsValid())
                {
                    throw new ArgumentException("Предоставленный JSON не соответствует схеме UnifiedValuationVector (отсутствуют обязательные поля).");
                }
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Невалидный формат JSON.", ex);
            }

            var cleanJson = JsonSerializer.Serialize(parsedData);

            if (existingCharacteristic != null)
            {
                existingCharacteristic.UpdateCharacteristics(cleanJson);
                await _context.SaveChangesAsync(cancellationToken);
                return existingCharacteristic.Id;
            }

            var newCharacteristic = new PropertyCharacteristic(request.ValuationUnitId, cleanJson);

            _context.PropertyCharacteristics.Add(newCharacteristic);
            await _context.SaveChangesAsync(cancellationToken);

            return newCharacteristic.Id;
        }
    }
}