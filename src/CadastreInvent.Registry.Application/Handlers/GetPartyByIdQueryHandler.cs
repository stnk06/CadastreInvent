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
    public class GetPartyByIdQueryHandler : IRequestHandler<GetPartyByIdQuery, PartyDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetPartyByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PartyDto> Handle(GetPartyByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Parties
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"Субъект (Party) с ID {request.Id} не найден.");

            return new PartyDto
            {
                Id = entity.Id,
                ExtId = entity.ExtId,
                Name = entity.Name,
                Type = entity.Type,
                ContactInfo = entity.ContactInfo
            };
        }
    }
}