using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Inspection.Application.Queries
{
    public record SearchSpatialUnitsQuery(string SearchTerm) : IRequest<List<LookupDto>>;

    public class LookupDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class SearchSpatialUnitsQueryHandler : IRequestHandler<SearchSpatialUnitsQuery, List<LookupDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public SearchSpatialUnitsQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<LookupDto>> Handle(SearchSpatialUnitsQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.SpatialUnits
                .AsNoTracking()
                .Where(s => s.Boundary != null);

            // Если передан поисковой запрос, фильтруем по номеру (ILike)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = $"%{request.SearchTerm}%";
                query = query.Where(s => EF.Functions.ILike(s.ReferenceNumber, searchTerm));
            }

            var results = await query
                .OrderBy(s => s.ReferenceNumber)
                .Take(50) // Загружаем до 50 записей в справочник
                .Select(s => new LookupDto
                {
                    Id = s.Id,
                    Text = $"Кадастровый номер: {s.ReferenceNumber} (Площадь: {s.AreaSqMeters:N1} м²)"
                })
                .ToListAsync(cancellationToken);

            return results;
        }
    }
}