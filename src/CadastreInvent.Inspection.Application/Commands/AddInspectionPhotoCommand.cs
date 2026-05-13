using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Inspection.Domain.Entities;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record AddInspectionPhotoCommand(
        Guid InspectionTaskId,
        string PhotoUrl,
        double Longitude,
        double Latitude,
        double Azimuth,
        DateTime CaptureDate,
        Guid? AppLocalId = null) : IRequest<Guid>;

    public class AddInspectionPhotoCommandHandler : IRequestHandler<AddInspectionPhotoCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly ICoordinateTransformationService _transformService;
        private readonly IInspectionNotificationService _notificationService;

        public AddInspectionPhotoCommandHandler(CadastreDbContext dbContext, ICoordinateTransformationService transformService, IInspectionNotificationService notificationService)
        {
            _dbContext = dbContext;
            _transformService = transformService;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(AddInspectionPhotoCommand request, CancellationToken cancellationToken)
        {
            if (request.AppLocalId.HasValue && request.AppLocalId.Value != Guid.Empty)
            {
                var existingPhoto = await _dbContext.InspectionPhotos
                    .FirstOrDefaultAsync(p => p.AppLocalId == request.AppLocalId.Value, cancellationToken);

                if (existingPhoto != null)
                {
                    return existingPhoto.Id;
                }
            }

            var task = await _dbContext.InspectionTasks.FindAsync(new object[] { request.InspectionTaskId }, cancellationToken);
            if (task == null) throw new Exception("Ордер инспекции не найден.");

            var photoPointWgs84 = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
            var targetPointMetric = _transformService.TransformFromWgs84(task.TargetCoordinates, 28408);
            var photoPointMetric = _transformService.TransformFromWgs84(photoPointWgs84, 28408);

            double distanceMeters = targetPointMetric.Distance(photoPointMetric);

            if (distanceMeters > 50)
            {
                task.SetGpsDiscrepancy(true);
            }

            if (task.RecordedCoordinates == null)
            {
                task.SetRecordedCoordinates(photoPointWgs84);
            }

            var photo = new InspectionPhoto(task.Id, request.PhotoUrl, request.AppLocalId);
            _dbContext.InspectionPhotos.Add(photo);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyPhotoAddedAsync(
                request.InspectionTaskId,
                photo.Id,
                request.PhotoUrl,
                request.Latitude,
                request.Longitude,
                request.CaptureDate,
                cancellationToken);

            return photo.Id;
        }
    }
}