using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class InitiateBatchRegistrationCommandHandler : IRequestHandler<InitiateBatchRegistrationCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public InitiateBatchRegistrationCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(InitiateBatchRegistrationCommand request, CancellationToken cancellationToken)
        {
            if (request.WktPolygons == null || request.WktPolygons.Count == 0)
                throw new ArgumentException();

            var job = new BatchRegistrationJob(request.WktPolygons.Count);

            foreach (var wkt in request.WktPolygons)
            {
                job.AddItem(wkt);
            }

            _dbContext.BatchRegistrationJobs.Add(job);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return job.Id;
        }
    }
}