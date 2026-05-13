using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using CadastreInvent.Infrastructure.Services.Excel;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Commands
{
    public record ParseExcelCommand(byte[] FileBytes, string FileName) : IRequest<ExcelPreviewResultDto>;

    public class ParseExcelCommandHandler : IRequestHandler<ParseExcelCommand, ExcelPreviewResultDto>
    {
        private readonly IExcelImportService _excelImportService;
        private readonly IDistributedCache _cache;
        private readonly CadastreDbContext _dbContext;

        public ParseExcelCommandHandler(IExcelImportService excelImportService, IDistributedCache cache, CadastreDbContext dbContext)
        {
            _excelImportService = excelImportService;
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<ExcelPreviewResultDto> Handle(ParseExcelCommand request, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream(request.FileBytes);

            var result = _excelImportService.ParseExcelFile(stream);
            result.FileName = request.FileName;

            var validCadNumbers = result.PreviewData
                .Where(r => r.IsValid)
                .Select(r => r.CadastralNumber)
                .Distinct()
                .ToList();

            var existingNumbers = await _dbContext.SpatialUnits
                .Where(s => validCadNumbers.Contains(s.ReferenceNumber))
                .Select(s => s.ReferenceNumber)
                .ToListAsync(cancellationToken);

            var existingSet = new HashSet<string>(existingNumbers);

            result.NewRows = 0;
            result.DuplicateRows = 0;

            foreach (var row in result.PreviewData.Where(r => r.IsValid))
            {
                if (existingSet.Contains(row.CadastralNumber))
                {
                    row.IsDuplicate = true;
                    result.DuplicateRows++;
                }
                else
                {
                    result.NewRows++;
                }
            }

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };

            var serializedData = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync($"ExcelImportSession_{result.SessionId}", serializedData, cacheOptions, cancellationToken);

            return result;
        }
    }
}