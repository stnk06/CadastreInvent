using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using CadastreInvent.Api.Hubs;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Api.Services
{
    public class InspectionNotificationService : IInspectionNotificationService
    {
        private readonly IHubContext<InspectionHub> _hubContext;

        public InspectionNotificationService(IHubContext<InspectionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTaskStatusChangedAsync(Guid taskId, string newStatus, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("TaskStatusChanged", taskId, newStatus, cancellationToken);
        }

        public async Task NotifyObservationAddedAsync(Guid taskId, Guid observationId, string category, string remarksJson, DateTime observationDate, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("ObservationAdded", taskId, new
            {
                id = observationId,
                category = category,
                remarksJson = remarksJson,
                observationDate = observationDate
            }, cancellationToken);
        }

        public async Task NotifyPhotoAddedAsync(Guid taskId, Guid photoId, string photoUrl, double latitude, double longitude, DateTime captureDate, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("PhotoAdded", taskId, new
            {
                id = photoId,
                photoUrl = photoUrl,
                lat = latitude,
                lon = longitude,
                captureDate = captureDate
            }, cancellationToken);
        }
    }
}