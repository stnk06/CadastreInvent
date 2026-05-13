using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Services
{
    public class MassAppraisalQueue : IMassAppraisalQueue
    {
        private readonly Channel<AppraisalRequest> _singleQueue;
        private readonly Channel<MassAppraisalJobContext> _jobQueue;

        private int _totalItems;
        private int _processedItems;
        private bool _isProcessing;

        public MassAppraisalQueue()
        {
            var singleOptions = new BoundedChannelOptions(10000) { FullMode = BoundedChannelFullMode.Wait };
            _singleQueue = Channel.CreateBounded<AppraisalRequest>(singleOptions);

            var jobOptions = new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.DropWrite };
            _jobQueue = Channel.CreateBounded<MassAppraisalJobContext>(jobOptions);
        }

        public async Task QueueSingleAppraisalAsync(AppraisalRequest request, CancellationToken cancellationToken = default)
        {
            await _singleQueue.Writer.WriteAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<AppraisalRequest> ReadAllSingleAsync(CancellationToken cancellationToken)
        {
            return _singleQueue.Reader.ReadAllAsync(cancellationToken);
        }

        public bool TryEnqueueJob(MassAppraisalJobContext context)
        {
            return _jobQueue.Writer.TryWrite(context);
        }

        public async Task<MassAppraisalJobContext> DequeueJobAsync(CancellationToken cancellationToken)
        {
            return await _jobQueue.Reader.ReadAsync(cancellationToken);
        }

        public void UpdateProgress(int total, int processed, bool isRunning)
        {
            _totalItems = total;
            _processedItems = processed;
            _isProcessing = isRunning;
        }

        public (int Total, int Processed, bool IsRunning) GetProgress()
        {
            return (_totalItems, _processedItems, _isProcessing);
        }
    }
}