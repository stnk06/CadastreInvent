using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record UpdatePartyContactInfoCommand(
        Guid PartyId,
        string NewContactInfo) : IRequest<bool>;
}