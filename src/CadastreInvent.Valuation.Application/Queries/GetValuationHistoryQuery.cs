using System;
using System.Collections.Generic;
using MediatR;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Queries
{
    public record GetValuationHistoryQuery(Guid ValuationId) : IRequest<List<ValuationHistoryDto>>;
}