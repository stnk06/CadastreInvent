using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Inspection.Domain.Entities;
using CadastreInvent.Inspection.Domain.Enums;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record AddInspectionObservationCommand(
        Guid InspectionTaskId,
        ObservationCategory Category,
        string RemarksJson,
        DateTime ObservationDate,
        Guid? AppLocalId = null) : IRequest<Guid>;

    public class AddInspectionObservationCommandHandler : IRequestHandler<AddInspectionObservationCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IInspectionNotificationService _notificationService;

        public AddInspectionObservationCommandHandler(CadastreDbContext dbContext, IInspectionNotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(AddInspectionObservationCommand request, CancellationToken cancellationToken)
        {
            if (request.AppLocalId.HasValue && request.AppLocalId.Value != Guid.Empty)
            {
                var existingObs = await _dbContext.InspectionObservations
                    .FirstOrDefaultAsync(o => o.AppLocalId == request.AppLocalId.Value, cancellationToken);

                if (existingObs != null)
                {
                    return existingObs.Id;
                }
            }

            string validRemarksJson = request.RemarksJson;
            if (!string.IsNullOrWhiteSpace(validRemarksJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(validRemarksJson);
                }
                catch (JsonException)
                {
                    validRemarksJson = JsonSerializer.Serialize(new { notes = request.RemarksJson });
                }
            }
            else
            {
                validRemarksJson = "{}";
            }

            var observation = new InspectionObservation(
                request.InspectionTaskId,
                request.Category,
                validRemarksJson,
                request.ObservationDate,
                request.AppLocalId);

            _dbContext.InspectionObservations.Add(observation);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyObservationAddedAsync(
                request.InspectionTaskId,
                observation.Id,
                observation.Category.ToString(),
                observation.RemarksJson,
                observation.ObservationDate,
                cancellationToken);

            return observation.Id;
        }
    }
}