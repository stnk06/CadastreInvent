using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.Commands;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class InvalidateSalesTransactionCommandHandler : IRequestHandler<InvalidateSalesTransactionCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public InvalidateSalesTransactionCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(InvalidateSalesTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _dbContext.SalesTransactions.FindAsync(new object[] { request.TransactionId }, cancellationToken);
            if (transaction == null) throw new Exception($"Сделка с ID {request.TransactionId} не найдена.");

            transaction.InvalidateTransaction(request.NewValidity);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}