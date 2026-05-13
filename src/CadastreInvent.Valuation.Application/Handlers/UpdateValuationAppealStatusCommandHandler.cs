using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class UpdateValuationAppealStatusCommandHandler : IRequestHandler<UpdateValuationAppealStatusCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public UpdateValuationAppealStatusCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateValuationAppealStatusCommand request, CancellationToken cancellationToken)
        {
            var appeal = await _dbContext.ValuationAppeals.FindAsync(new object[] { request.AppealId }, cancellationToken);
            if (appeal == null) throw new Exception($"Апелляция с ID {request.AppealId} не найдена.");

            appeal.UpdateStatus(request.NewStatus);

            if (request.NewStatus == AppealStatus.Resolved && request.NewAssessedValue.HasValue)
            {
                var valuation = await _dbContext.Valuations.FindAsync(new object[] { appeal.ValuationId }, cancellationToken);
                if (valuation == null) throw new Exception($"Оценка с ID {appeal.ValuationId} не найдена.");

                valuation.ApplyAppealDecision(request.NewAssessedValue.Value);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}