using System;
using MediatR;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Queries
{
    public record GetSalesTransactionByIdQuery(Guid Id) : IRequest<SalesTransactionDto>;
}