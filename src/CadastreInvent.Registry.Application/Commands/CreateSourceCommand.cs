using System;
using MediatR;
using CadastreInvent.Registry.Domain.Enums;

namespace CadastreInvent.Registry.Application.Commands
{
    public record CreateSourceCommand(
        SourceType Type,
        string DocumentNumber,
        DateTime RecordDate,
        string ContentUrl) : IRequest<Guid>;
}