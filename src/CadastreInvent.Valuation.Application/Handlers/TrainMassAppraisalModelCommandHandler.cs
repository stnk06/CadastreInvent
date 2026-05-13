using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Application.Services;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class TrainMassAppraisalModelCommandHandler : IRequestHandler<TrainMassAppraisalModelCommand, Guid>
    {
        private readonly IMlTrainingQueue _trainingQueue;

        public TrainMassAppraisalModelCommandHandler(IMlTrainingQueue trainingQueue)
        {
            _trainingQueue = trainingQueue;
        }

        public Task<Guid> Handle(TrainMassAppraisalModelCommand request, CancellationToken cancellationToken)
        {
            var jobId = Guid.NewGuid();

            var context = new MlTrainingJobContext
            {
                JobId = jobId,
                Version = request.Version,
                Description = request.Description
            };

            bool enqueued = _trainingQueue.TryEnqueueTrainingJob(context);

            if (!enqueued)
            {
                throw new InvalidOperationException("Очередь процессов обучения переполнена. Пожалуйста, повторите попытку позже.");
            }

            return Task.FromResult(jobId);
        }
    }
}