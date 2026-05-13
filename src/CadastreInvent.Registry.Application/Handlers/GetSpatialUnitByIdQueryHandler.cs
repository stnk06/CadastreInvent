using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Registry.Application.Queries;
using CadastreInvent.Registry.Application.DTOs;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class GetSpatialUnitByIdQueryHandler : IRequestHandler<GetSpatialUnitByIdQuery, SpatialUnitDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetSpatialUnitByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SpatialUnitDto> Handle(GetSpatialUnitByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.SpatialUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                throw new System.Exception($"Участок с ID {request.Id} не найден");
            }

            return new SpatialUnitDto
            {
                Id = entity.Id,
                ReferenceNumber = entity.ReferenceNumber,
                Type = entity.Type,
                BoundaryWkt = entity.Boundary.ToString(),
                AreaSqMeters = entity.AreaSqMeters
            };
        }
    }
}