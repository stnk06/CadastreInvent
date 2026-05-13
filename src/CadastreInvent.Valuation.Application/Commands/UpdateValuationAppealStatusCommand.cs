using System;
using MediatR;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record UpdateValuationAppealStatusCommand(
        Guid AppealId,
        AppealStatus NewStatus,
        decimal? NewAssessedValue = null) : IRequest<bool>;
}