using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class CreatePartyGroupCommandHandler : IRequestHandler<CreatePartyGroupCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreatePartyGroupCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreatePartyGroupCommand request, CancellationToken cancellationToken)
        {
            var partyGroup = new PartyGroup(request.Name);

            _dbContext.PartyGroups.Add(partyGroup);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return partyGroup.Id;
        }
    }
}