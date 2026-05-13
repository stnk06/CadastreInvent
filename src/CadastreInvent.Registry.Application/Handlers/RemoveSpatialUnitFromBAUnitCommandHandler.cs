using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class RemoveSpatialUnitFromBAUnitCommandHandler : IRequestHandler<RemoveSpatialUnitFromBAUnitCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public RemoveSpatialUnitFromBAUnitCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(RemoveSpatialUnitFromBAUnitCommand request, CancellationToken cancellationToken)
        {
            var baUnit = await _dbContext.BAUnits.FirstOrDefaultAsync(b => b.Id == request.BAUnitId, cancellationToken);
            if (baUnit == null) throw new Exception($"BAUnit с ID {request.BAUnitId} не найден.");

            baUnit.RemoveSpatialUnit(request.SpatialUnitId);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}