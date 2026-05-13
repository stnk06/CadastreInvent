using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class CreateValuationAppealCommandHandler : IRequestHandler<CreateValuationAppealCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreateValuationAppealCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateValuationAppealCommand request, CancellationToken cancellationToken)
        {
            var appeal = new ValuationAppeal(
                request.ValuationId,
                request.ApplicantPartyId,
                request.Reason,
                DateTime.UtcNow);

            _dbContext.ValuationAppeals.Add(appeal);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return appeal.Id;
        }
    }
}