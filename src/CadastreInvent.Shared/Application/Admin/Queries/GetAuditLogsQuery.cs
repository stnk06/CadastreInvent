using System.Collections.Generic;
using MediatR;
using CadastreInvent.Shared.Application.Admin.DTOs;

namespace CadastreInvent.Shared.Application.Admin.Queries
{
    public record GetAuditLogsQuery : IRequest<List<AuditLogDto>>;
}