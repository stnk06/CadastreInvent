using System;
using MediatR;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.Commands
{
    public record RegisterRRRCommand(
        RRRType Type,
        Guid BAUnitId,
        Guid? PartyId,
        Guid? PartyGroupId,
        Guid SourceId,
        decimal ShareNumerator,
        decimal ShareDenominator,
        DateTime StartDate) : IRequest<Guid>;
}