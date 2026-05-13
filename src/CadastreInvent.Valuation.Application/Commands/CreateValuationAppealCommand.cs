using System;
using MediatR;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record CreateValuationAppealCommand(
        Guid ValuationId,
        Guid ApplicantPartyId,
        string Reason) : IRequest<Guid>;
}