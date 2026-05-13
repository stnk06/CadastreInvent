using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Registry.Application.Handlers
{
    public class CreateBAUnitCommandHandler : IRequestHandler<CreateBAUnitCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreateBAUnitCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateBAUnitCommand request, CancellationToken cancellationToken)
        {
            var baUnit = new BAUnit(request.Name, request.Type);

            _dbContext.BAUnits.Add(baUnit);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return baUnit.Id;
        }
    }
}