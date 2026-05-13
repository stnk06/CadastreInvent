using System;
using MediatR;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.Commands
{
    public record CreateBAUnitCommand(
        string Name,
        BAUnitType Type) : IRequest<Guid>;
}