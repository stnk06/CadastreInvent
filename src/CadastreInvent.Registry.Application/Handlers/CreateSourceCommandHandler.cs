using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class CreateSourceCommandHandler : IRequestHandler<CreateSourceCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreateSourceCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateSourceCommand request, CancellationToken cancellationToken)
        {
            var source = new Source(
                request.Type,
                request.DocumentNumber,
                request.RecordDate,
                request.ContentUrl ?? string.Empty);

            _dbContext.Sources.Add(source);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return source.Id;
        }
    }
}