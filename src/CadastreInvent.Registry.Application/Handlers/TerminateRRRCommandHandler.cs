using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class TerminateRRRCommandHandler : IRequestHandler<TerminateRRRCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public TerminateRRRCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(TerminateRRRCommand request, CancellationToken cancellationToken)
        {
            var baUnit = await _dbContext.BAUnits
                .Include(b => b.Rrrs)
                .FirstOrDefaultAsync(b => b.Id == request.BAUnitId, cancellationToken);

            if (baUnit == null) throw new Exception($"BAUnit с ID {request.BAUnitId} не найден.");

            baUnit.TerminateRRR(request.RRRId, request.EndDate);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}