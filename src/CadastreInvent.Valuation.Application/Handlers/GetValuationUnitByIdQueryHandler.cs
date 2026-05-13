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
    public class GetValuationUnitByIdQueryHandler : IRequestHandler<GetValuationUnitByIdQuery, ValuationUnitDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetValuationUnitByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ValuationUnitDto> Handle(GetValuationUnitByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.ValuationUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"Единица оценки с ID {request.Id} не найдена.");

            return new ValuationUnitDto
            {
                Id = entity.Id,
                BAUnitId = entity.BAUnitId,
                ZoningStatus = entity.ZoningStatus
            };
        }
    }
}