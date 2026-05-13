using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Admin.DTOs;
using CadastreInvent.Shared.Application.Admin.Queries;

namespace CadastreInvent.Api.Admin.Handlers
{
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public GetAuditLogsQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(100)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Action = a.Action,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId.ToString(),
                    Changes = a.ChangesJson ?? string.Empty,
                    Timestamp = a.Timestamp
                })
                .ToListAsync(cancellationToken);
        }
    }
}