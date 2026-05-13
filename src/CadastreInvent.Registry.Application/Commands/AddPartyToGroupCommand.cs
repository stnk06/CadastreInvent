using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record AddPartyToGroupCommand(
        Guid PartyGroupId,
        Guid PartyId,
        decimal ShareNumerator,
        decimal ShareDenominator) : IRequest<bool>;
}