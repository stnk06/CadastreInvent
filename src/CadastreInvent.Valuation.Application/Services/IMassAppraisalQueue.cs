using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Services
{
    public interface IMassAppraisalQueue
    {
        Task QueueSingleAppraisalAsync(AppraisalRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<AppraisalRequest> ReadAllSingleAsync(CancellationToken cancellationToken);
        bool TryEnqueueJob(MassAppraisalJobContext context);
        Task<MassAppraisalJobContext> DequeueJobAsync(CancellationToken cancellationToken);
        void UpdateProgress(int total, int processed, bool isRunning);

        (int Total, int Processed, bool IsRunning) GetProgress();
    }
}