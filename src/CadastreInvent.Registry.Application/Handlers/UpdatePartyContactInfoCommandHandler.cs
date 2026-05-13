using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class UpdatePartyContactInfoCommandHandler : IRequestHandler<UpdatePartyContactInfoCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public UpdatePartyContactInfoCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdatePartyContactInfoCommand request, CancellationToken cancellationToken)
        {
            var party = await _dbContext.Parties.FindAsync(new object[] { request.PartyId }, cancellationToken);
            if (party == null) throw new Exception($"Субъект с ID {request.PartyId} не найден.");

            party.UpdateContactInfo(request.NewContactInfo);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}