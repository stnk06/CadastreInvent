using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class AddSpatialUnitToBAUnitCommandHandler : IRequestHandler<AddSpatialUnitToBAUnitCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public AddSpatialUnitToBAUnitCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(AddSpatialUnitToBAUnitCommand request, CancellationToken cancellationToken)
        {
            var baUnit = await _dbContext.BAUnits.FirstOrDefaultAsync(b => b.Id == request.BAUnitId, cancellationToken);
            if (baUnit == null)
            {
                throw new Exception($"BAUnit с ID {request.BAUnitId} не найден.");
            }

            bool spatialUnitExists = await _dbContext.SpatialUnits.AnyAsync(s => s.Id == request.SpatialUnitId, cancellationToken);
            if (!spatialUnitExists)
            {
                throw new Exception($"Пространственный контур с ID {request.SpatialUnitId} не найден в базе данных.");
            }

            baUnit.AddSpatialUnit(request.SpatialUnitId);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}