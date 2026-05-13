using System;
using MediatR;
using CadastreInvent.Inspection.Application.DTOs;

namespace CadastreInvent.Inspection.Application.Queries
{
    public record GetInspectionPhotoByIdQuery(Guid Id) : IRequest<InspectionPhotoDto?>;
}