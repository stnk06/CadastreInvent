using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Registry.Application.Queries;
using CadastreInvent.Registry.Application.DTOs;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class GetBAUnitByIdQueryHandler : IRequestHandler<GetBAUnitByIdQuery, BAUnitDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetBAUnitByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BAUnitDto> Handle(GetBAUnitByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.BAUnits
                .Include(x => x.Rrrs)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"BAUnit с ID {request.Id} не найден.");

            return new BAUnitDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                SpatialUnitIds = entity.SpatialUnits.Select(su => su.SpatialUnitId).ToList(),
                Rrrs = entity.Rrrs.Select(r => new RRRDto
                {
                    Id = r.Id,
                    Type = r.Type,
                    BAUnitId = r.BAUnitId,
                    PartyId = r.PartyId,
                    PartyGroupId = r.PartyGroupId,
                    SourceId = r.SourceId,
                    ShareNumerator = r.ShareNumerator,
                    ShareDenominator = r.ShareDenominator,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate
                }).ToList()
            };
        }
    }
}