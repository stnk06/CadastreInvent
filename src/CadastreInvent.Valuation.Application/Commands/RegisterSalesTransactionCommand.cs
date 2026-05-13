using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record RegisterSalesTransactionCommand(
        Guid ValuationUnitId,
        decimal SalePrice,
        DateTime TransactionDate) : IRequest<Guid>;
}