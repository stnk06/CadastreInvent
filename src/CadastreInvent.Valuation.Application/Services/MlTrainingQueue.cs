using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Services
{
    public interface IMlTrainingQueue
    {
        bool TryEnqueueTrainingJob(MlTrainingJobContext context);
        ValueTask<MlTrainingJobContext> DequeueJobAsync(CancellationToken cancellationToken);
    }

    public class MlTrainingQueue : IMlTrainingQueue
    {
        private readonly Channel<MlTrainingJobContext> _jobQueue;
        private readonly ILogger<MlTrainingQueue> _logger;

        public MlTrainingQueue(ILogger<MlTrainingQueue> logger)
        {
            _logger = logger;
            _jobQueue = Channel.CreateBounded<MlTrainingJobContext>(new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.DropWrite });
        }

        public bool TryEnqueueTrainingJob(MlTrainingJobContext context)
        {
            if (context == null) return false;
            var success = _jobQueue.Writer.TryWrite(context);
            if (!success) _logger.LogWarning("Очередь обучения ML переполнена. Задача отклонена.");
            return success;
        }

        public ValueTask<MlTrainingJobContext> DequeueJobAsync(CancellationToken cancellationToken) => _jobQueue.Reader.ReadAsync(cancellationToken);
    }
}