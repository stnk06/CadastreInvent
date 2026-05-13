using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Registry.Application.Queries;
using CadastreInvent.Registry.Application.DTOs;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class GetPartyGroupByIdQueryHandler : IRequestHandler<GetPartyGroupByIdQuery, PartyGroupDto>
    {
        private readonly CadastreDbContext _dbContext;

        public GetPartyGroupByIdQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PartyGroupDto> Handle(GetPartyGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.PartyGroups
                .Include(x => x.Members)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null) throw new Exception($"Группа субъектов с ID {request.Id} не найдена.");

            return new PartyGroupDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Members = entity.Members.Select(m => new PartyGroupMemberDto
                {
                    PartyId = m.PartyId,
                    ShareNumerator = m.ShareNumerator,
                    ShareDenominator = m.ShareDenominator
                }).ToList()
            };
        }
    }
}