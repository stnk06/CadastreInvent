using System;
using System.Collections.Generic;
using MediatR;

namespace CadastreInvent.Registry.Application.Commands
{
    public record InitiateBatchRegistrationCommand(List<string> WktPolygons) : IRequest<Guid>;
}