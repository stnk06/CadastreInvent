using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class RegisterPartyCommandHandler : IRequestHandler<RegisterPartyCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public RegisterPartyCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(RegisterPartyCommand request, CancellationToken cancellationToken)
        {
            var cleanExtId = request.ExtId.Replace("-", "").Replace(" ", "");

            var cleanContactInfo = request.ContactType == "Телефон"
                ? request.ContactInfo.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")
                : request.ContactInfo;

            var party = new Party(cleanExtId, request.Name, request.Type, cleanContactInfo);

            _dbContext.Parties.Add(party);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return party.Id;
        }
    }
}