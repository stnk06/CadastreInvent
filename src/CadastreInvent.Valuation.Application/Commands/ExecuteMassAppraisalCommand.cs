using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record ExecuteMassAppraisalCommand(
        Guid ModelId,
        string? ZoningStatusFilter = null,
        float? MinArea = null,
        float? MaxArea = null
    ) : IRequest<Guid>;
}