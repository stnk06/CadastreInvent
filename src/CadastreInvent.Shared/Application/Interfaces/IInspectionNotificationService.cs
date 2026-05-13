using System;
using System.Threading;
using System.Threading.Tasks;

namespace CadastreInvent.Shared.Application.Interfaces
{
    public interface IInspectionNotificationService
    {
        Task NotifyTaskStatusChangedAsync(Guid taskId, string newStatus, CancellationToken cancellationToken);
        Task NotifyObservationAddedAsync(Guid taskId, Guid observationId, string category, string remarksJson, DateTime observationDate, CancellationToken cancellationToken);
        Task NotifyPhotoAddedAsync(Guid taskId, Guid photoId, string photoUrl, double latitude, double longitude, DateTime captureDate, CancellationToken cancellationToken);
    }
}