using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class RemovePartyFromGroupCommandHandler : IRequestHandler<RemovePartyFromGroupCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public RemovePartyFromGroupCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(RemovePartyFromGroupCommand request, CancellationToken cancellationToken)
        {
            int maxRetries = 3;
            int delayMs = 100;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var group = await _dbContext.PartyGroups
                        .Include(g => g.Members)
                        .FirstOrDefaultAsync(g => g.Id == request.PartyGroupId, cancellationToken);

                    if (group == null)
                        throw new Exception($"Группа субъектов с ID {request.PartyGroupId} не найдена.");

                    group.RemoveMember(request.PartyId);

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == maxRetries)
                    {
                        throw new InvalidOperationException("Не удалось исключить субъекта из-за параллельных изменений. Пожалуйста, повторите попытку.");
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