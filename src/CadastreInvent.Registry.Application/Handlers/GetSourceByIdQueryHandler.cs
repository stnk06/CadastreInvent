using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Registry.Application.Queries;
using CadastreInvent.Registry.Application.DTOs;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class GetSourceByIdQueryHandler : IRequestHandler<GetSourceByIdQuery, SourceDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetSourceByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SourceDto> Handle(GetSourceByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Sources
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"Документ (Source) с ID {request.Id} не найден.");

            return new SourceDto
            {
                Id = entity.Id,
                Type = entity.Type,
                DocumentNumber = entity.DocumentNumber,
                RecordDate = entity.RecordDate,
                ContentUrl = entity.ContentUrl
            };
        }
    }
}