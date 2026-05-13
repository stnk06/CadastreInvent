using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.DTOs;
using CadastreInvent.Valuation.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class GetSalesTransactionByIdQueryHandler : IRequestHandler<GetSalesTransactionByIdQuery, SalesTransactionDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetSalesTransactionByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SalesTransactionDto> Handle(GetSalesTransactionByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.SalesTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"Сделка с ID {request.Id} не найдена.");

            return new SalesTransactionDto
            {
                Id = entity.Id,
                ValuationUnitId = entity.ValuationUnitId,
                SalePrice = entity.SalePrice,
                TransactionDate = entity.TransactionDate,
                Validity = entity.Validity
            };
        }
    }
}