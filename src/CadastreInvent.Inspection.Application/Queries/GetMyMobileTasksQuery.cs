using System;
using System.Collections.Generic;
using MediatR;
using CadastreInvent.Inspection.Application.DTOs;

namespace CadastreInvent.Inspection.Application.Queries
{
    public record GetMyMobileTasksQuery(Guid InspectorId) : IRequest<List<MobileTaskDto>>;
}