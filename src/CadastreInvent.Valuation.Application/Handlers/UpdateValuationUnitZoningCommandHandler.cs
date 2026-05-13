using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.Commands;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class UpdateValuationUnitZoningCommandHandler : IRequestHandler<UpdateValuationUnitZoningCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public UpdateValuationUnitZoningCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateValuationUnitZoningCommand request, CancellationToken cancellationToken)
        {
            var unit = await _dbContext.ValuationUnits.FindAsync(new object[] { request.ValuationUnitId }, cancellationToken);
            if (unit == null) throw new Exception($"Объект оценки с ID {request.ValuationUnitId} не найден.");

            unit.UpdateZoningStatus(request.ZoningStatus);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}