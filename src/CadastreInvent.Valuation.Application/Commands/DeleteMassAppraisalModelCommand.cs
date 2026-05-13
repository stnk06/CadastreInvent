using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Commands
{
    public record DeleteMassAppraisalModelCommand(Guid ModelId) : IRequest;

    public class DeleteMassAppraisalModelCommandHandler : IRequestHandler<DeleteMassAppraisalModelCommand>
    {
        private readonly CadastreDbContext _dbContext;

        public DeleteMassAppraisalModelCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(DeleteMassAppraisalModelCommand request, CancellationToken cancellationToken)
        {
            var model = await _dbContext.MassAppraisalModels
                .FirstOrDefaultAsync(m => m.Id == request.ModelId, cancellationToken);

            if (model == null) return;

            _dbContext.Entry(model).Property("IsDeleted").CurrentValue = true;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}