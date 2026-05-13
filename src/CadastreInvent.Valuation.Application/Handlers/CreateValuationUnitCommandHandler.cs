using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class CreateValuationUnitCommandHandler : IRequestHandler<CreateValuationUnitCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreateValuationUnitCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateValuationUnitCommand request, CancellationToken cancellationToken)
        {
            var valuationUnit = new ValuationUnit(request.BAUnitId, request.ZoningStatus);

            _dbContext.ValuationUnits.Add(valuationUnit);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return valuationUnit.Id;
        }
    }
}