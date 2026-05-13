using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Domain.Events;
using CadastreInvent.Valuation.Application.DTOs;
using CadastreInvent.Valuation.Application.Services;

namespace CadastreInvent.Valuation.Application.EventHandlers
{
    public class PropertyCharacteristicsChangedEventHandler : INotificationHandler<PropertyCharacteristicsChangedEvent>
    {
        private readonly IMassAppraisalQueue _appraisalQueue;
        private readonly CadastreDbContext _dbContext;

        public PropertyCharacteristicsChangedEventHandler(IMassAppraisalQueue appraisalQueue, CadastreDbContext dbContext)
        {
            _appraisalQueue = appraisalQueue;
            _dbContext = dbContext;
        }

        public async Task Handle(PropertyCharacteristicsChangedEvent notification, CancellationToken cancellationToken)
        {
            var spatialUnitExists = await _dbContext.SpatialUnits
                .AsNoTracking()
                .AnyAsync(s => s.Id == notification.SpatialUnitId, cancellationToken);

            if (!spatialUnitExists) return;

            var request = new AppraisalRequest(notification.SpatialUnitId);

            await _appraisalQueue.QueueSingleAppraisalAsync(request, cancellationToken);
        }
    }
}