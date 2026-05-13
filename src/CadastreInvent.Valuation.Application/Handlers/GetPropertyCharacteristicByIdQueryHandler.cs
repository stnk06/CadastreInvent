using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Valuation.Application.Queries;
using CadastreInvent.Valuation.Application.DTOs;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class GetPropertyCharacteristicByIdQueryHandler : IRequestHandler<GetPropertyCharacteristicByIdQuery, PropertyCharacteristicDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetPropertyCharacteristicByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PropertyCharacteristicDto> Handle(GetPropertyCharacteristicByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.PropertyCharacteristics
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"Характеристика с ID {request.Id} не найдена.");

            return new PropertyCharacteristicDto
            {
                Id = entity.Id,
                ValuationUnitId = entity.ValuationUnitId,
                CharacteristicsJson = entity.CharacteristicsJson
            };
        }
    }
}