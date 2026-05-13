using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Registry.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class RegisterRRRCommandHandler : IRequestHandler<RegisterRRRCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public RegisterRRRCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(RegisterRRRCommand request, CancellationToken cancellationToken)
        {
            bool baUnitExists = await _dbContext.BAUnits.AnyAsync(b => b.Id == request.BAUnitId, cancellationToken);
            if (!baUnitExists)
            {
                throw new InvalidOperationException($"Объект недвижимости с ID {request.BAUnitId} не найден.");
            }

            if (request.PartyId.HasValue && request.PartyId.Value != Guid.Empty)
            {
                bool partyExists = await _dbContext.Parties.AnyAsync(p => p.Id == request.PartyId.Value, cancellationToken);
                if (!partyExists) throw new InvalidOperationException($"Субъект права с ID {request.PartyId} не найден.");
            }
            else if (request.PartyGroupId.HasValue && request.PartyGroupId.Value != Guid.Empty)
            {
                bool groupExists = await _dbContext.PartyGroups.AnyAsync(g => g.Id == request.PartyGroupId.Value, cancellationToken);
                if (!groupExists) throw new InvalidOperationException($"Группа субъектов с ID {request.PartyGroupId} не найдена.");
            }

            bool sourceExists = await _dbContext.Sources.AnyAsync(s => s.Id == request.SourceId, cancellationToken);
            if (!sourceExists)
            {
                throw new InvalidOperationException($"Документ-основание с ID {request.SourceId} не найден в БД.");
            }

            var existingShares = await _dbContext.Rrrs
                .Where(r => r.BAUnitId == request.BAUnitId &&
                            r.Type == request.Type &&
                            (!r.EndDate.HasValue || r.EndDate > DateTime.UtcNow))
                .Select(r => new { r.ShareNumerator, r.ShareDenominator })
                .ToListAsync(cancellationToken);

            decimal currentTotalShare = 0m;

            foreach (var share in existingShares)
            {
                if (share.ShareDenominator > 0)
                {
                    currentTotalShare += share.ShareNumerator / share.ShareDenominator;
                }
            }

            decimal requestedShare = request.ShareDenominator > 0 ? request.ShareNumerator / request.ShareDenominator : 0m;
            decimal projectedTotalShare = currentTotalShare + requestedShare;

            if (projectedTotalShare > 1.0m)
            {
                throw new InvalidOperationException($"Регистрация отклонена. Общая сумма долей права '{request.Type}' превысит 100%. Текущая зарегистрированная доля: {currentTotalShare * 100:0.##}%, запрошенная доля: {requestedShare * 100:0.##}%.");
            }

            var rrr = new RRR(
                request.Type,
                request.BAUnitId,
                request.SourceId,
                request.ShareNumerator,
                request.ShareDenominator,
                request.StartDate,
                request.PartyId,
                request.PartyGroupId);

            _dbContext.Rrrs.Add(rrr);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return rrr.Id;
        }
    }
}