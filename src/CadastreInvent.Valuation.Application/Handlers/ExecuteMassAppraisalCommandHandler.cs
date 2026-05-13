using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Application.Services;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class ExecuteMassAppraisalCommandHandler : IRequestHandler<ExecuteMassAppraisalCommand, Guid>
    {
        private readonly IMassAppraisalQueue _appraisalQueue;

        public ExecuteMassAppraisalCommandHandler(IMassAppraisalQueue appraisalQueue)
        {
            _appraisalQueue = appraisalQueue;
        }

        public Task<Guid> Handle(ExecuteMassAppraisalCommand request, CancellationToken cancellationToken)
        {
            var jobId = Guid.NewGuid();

            var context = new MassAppraisalJobContext
            {
                JobId = jobId,
                ModelId = request.ModelId,
                ZoningStatusFilter = request.ZoningStatusFilter,
                MinArea = request.MinArea,
                MaxArea = request.MaxArea
            };

            bool enqueued = _appraisalQueue.TryEnqueueJob(context);

            if (!enqueued)
            {
                throw new InvalidOperationException("Очередь массовой оценки переполнена. Пожалуйста, повторите попытку позже.");
            }

            return Task.FromResult(jobId);
        }
    }
}