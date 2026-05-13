using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.DTOs;
using CadastreInvent.Registry.Application.Queries;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class GetBatchJobStatusQueryHandler : IRequestHandler<GetBatchJobStatusQuery, BatchJobStatusDto?>
    {
        private readonly CadastreDbContext _dbContext;

        public GetBatchJobStatusQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BatchJobStatusDto?> Handle(GetBatchJobStatusQuery request, CancellationToken cancellationToken)
        {
            var job = await _dbContext.BatchRegistrationJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

            if (job == null) return null;

            return new BatchJobStatusDto
            {
                JobId = job.Id,
                TotalCount = job.TotalCount,
                ProcessedCount = job.ProcessedCount,
                Status = job.Status.ToString()
            };
        }
    }
}