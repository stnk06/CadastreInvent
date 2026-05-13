using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Queries
{
    public record ImportHistoryDto(Guid Id, DateTime ImportDateUtc, string FileName, int TotalRows, int ImportedRows, string UserName);

    public record GetImportHistoryQuery() : IRequest<List<ImportHistoryDto>>;

    public class GetImportHistoryQueryHandler : IRequestHandler<GetImportHistoryQuery, List<ImportHistoryDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public GetImportHistoryQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ImportHistoryDto>> Handle(GetImportHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.ImportHistories
                .AsNoTracking()
                .OrderByDescending(h => h.ImportDateUtc)
                .Take(20) 
                .Select(h => new ImportHistoryDto(
                    h.Id,
                    h.ImportDateUtc,
                    h.FileName,
                    h.TotalRows,
                    h.ImportedRows,
                    h.UserName
                ))
                .ToListAsync(cancellationToken);
        }
    }
}