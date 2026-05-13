using System;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record CreatePartyGroupCommand(string Name) : IRequest<Guid>;
}