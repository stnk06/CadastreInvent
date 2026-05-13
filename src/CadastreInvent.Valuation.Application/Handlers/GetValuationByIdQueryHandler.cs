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
    public class GetValuationByIdQueryHandler : IRequestHandler<GetValuationByIdQuery, ValuationDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetValuationByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ValuationDto> Handle(GetValuationByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Valuations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"Оценка с ID {request.Id} не найдена.");

            return new ValuationDto
            {
                Id = entity.Id,
                ValuationUnitId = entity.ValuationUnitId,
                ModelId = entity.ModelId,
                AssessedValue = entity.AssessedValue,
                ValuationDate = entity.ValuationDate,
                Method = entity.Method
            };
        }
    }
}