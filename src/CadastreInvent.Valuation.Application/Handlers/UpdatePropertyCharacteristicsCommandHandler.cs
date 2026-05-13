using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Valuation.Application.Commands;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CadastreInvent.Valuation.Application.Handlers
{
    public class UpdatePropertyCharacteristicsCommandHandler : IRequestHandler<UpdatePropertyCharacteristicsCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public UpdatePropertyCharacteristicsCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdatePropertyCharacteristicsCommand request, CancellationToken cancellationToken)
        {
            var characteristics = await _dbContext.PropertyCharacteristics.FindAsync(new object[] { request.CharacteristicId }, cancellationToken);
            if (characteristics == null) throw new Exception($"Характеристика с ID {request.CharacteristicId} не найдена.");

            characteristics.UpdateCharacteristics(request.CharacteristicsJson);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}