using System;
using MediatR;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record InvalidateSalesTransactionCommand(
        Guid TransactionId,
        TransactionValidity NewValidity) : IRequest<bool>;
}