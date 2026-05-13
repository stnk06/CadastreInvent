using System;
using MediatR;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Queries
{
    public record GetSourceByIdQuery(Guid Id) : IRequest<SourceDto>;
}