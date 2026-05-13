using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class AddPartyToGroupCommandHandler : IRequestHandler<AddPartyToGroupCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public AddPartyToGroupCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(AddPartyToGroupCommand request, CancellationToken cancellationToken)
        {
            int maxRetries = 3;
            int delayMs = 100;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var partyGroup = await _dbContext.PartyGroups
                        .Include(pg => pg.Members)
                        .FirstOrDefaultAsync(pg => pg.Id == request.PartyGroupId, cancellationToken);

                    if (partyGroup == null)
                        throw new Exception($"Группа с ID {request.PartyGroupId} не найдена.");

                    var party = await _dbContext.Parties.FindAsync(new object[] { request.PartyId }, cancellationToken);
                    if (party == null)
                        throw new Exception($"Субъект с ID {request.PartyId} не найден.");

                    partyGroup.AddMember(party, request.ShareNumerator, request.ShareDenominator);

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == maxRetries)
                    {
                        throw new InvalidOperationException("Не удалось обновить состав группы из-за параллельных изменений другими операторами. Пожалуйста, обновите страницу и повторите попытку.");
                    }

                    _dbContext.ChangeTracker.Clear();

                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2;
                }
            }

            return false;
        }
    }
}