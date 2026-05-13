using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Domain.Enums;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class RegisterSalesTransactionCommandHandler : IRequestHandler<RegisterSalesTransactionCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public RegisterSalesTransactionCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(RegisterSalesTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = new SalesTransaction(
                request.ValuationUnitId,
                request.SalePrice,
                request.TransactionDate,
                TransactionValidity.ValidMarket);

            _dbContext.SalesTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return transaction.Id;
        }
    }
}