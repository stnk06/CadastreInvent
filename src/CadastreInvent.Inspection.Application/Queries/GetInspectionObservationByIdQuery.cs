using System;
using MediatR;
using CadastreInvent.Inspection.Application.DTOs;

namespace CadastreInvent.Inspection.Application.Queries
{
    public record GetInspectionObservationByIdQuery(Guid Id) : IRequest<InspectionObservationDto?>;
}