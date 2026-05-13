using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record HardDeleteMassAppraisalModelCommand(Guid ModelId) : IRequest;

    public class HardDeleteMassAppraisalModelCommandHandler : IRequestHandler<HardDeleteMassAppraisalModelCommand>
    {
        private readonly CadastreDbContext _dbContext;

        public HardDeleteMassAppraisalModelCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(HardDeleteMassAppraisalModelCommand request, CancellationToken cancellationToken)
        {
            var model = await _dbContext.MassAppraisalModels.IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == request.ModelId, cancellationToken);

            if (model == null) return;

            await _dbContext.Valuations
                .Where(v => v.ModelId == request.ModelId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ModelId, (Guid?)null), cancellationToken);

            _dbContext.MassAppraisalModels.Remove(model);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}