using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record RemovePartyFromGroupCommand(
        Guid PartyGroupId,
        Guid PartyId) : IRequest<bool>;
}