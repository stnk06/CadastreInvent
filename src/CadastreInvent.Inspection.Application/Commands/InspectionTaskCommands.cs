using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Inspection.Domain.Entities;
using CadastreInvent.Inspection.Domain.Enums;
using CadastreInvent.Shared.Application.Interfaces;
using CadastreInvent.Shared.Domain.Events;

namespace CadastreInvent.Inspection.Application.Commands
{
    public record CreateInspectionTaskCommand(Guid SpatialUnitId, Guid InspectorId, DateTime Deadline) : IRequest<Guid>;

    public class CreateInspectionTaskCommandHandler : IRequestHandler<CreateInspectionTaskCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreateInspectionTaskCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateInspectionTaskCommand request, CancellationToken cancellationToken)
        {
            var activeTaskExists = await _dbContext.InspectionTasks
                .AnyAsync(t => t.TargetSpatialUnitId == request.SpatialUnitId &&
                               (t.State == TaskState.Created || t.State == TaskState.Assigned || t.State == TaskState.InProgress),
                               cancellationToken);

            if (activeTaskExists)
            {
                throw new InvalidOperationException("Для данного участка уже существует активный ордер инспекции. Завершите или аннулируйте текущий ордер перед созданием нового.");
            }

            var spatialUnit = await _dbContext.SpatialUnits.FindAsync(new object[] { request.SpatialUnitId }, cancellationToken);

            if (spatialUnit == null || spatialUnit.Boundary == null)
            {
                throw new Exception("Целевой кадастровый участок не найден или не имеет физических границ (геометрии) в базе данных PostGIS.");
            }

            var centroid = spatialUnit.Boundary.Centroid;
            centroid.SRID = 4326;

            var task = new InspectionTask(request.SpatialUnitId, centroid, "Плановое обследование контура", request.Deadline);
            task.AssignTo(request.InspectorId);

            _dbContext.InspectionTasks.Add(task);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return task.Id;
        }
    }

    public record StartInspectionTaskCommand(Guid TaskId) : IRequest<bool>;

    public class StartInspectionTaskCommandHandler : IRequestHandler<StartInspectionTaskCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IInspectionNotificationService _notificationService;

        public StartInspectionTaskCommandHandler(CadastreDbContext dbContext, IInspectionNotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(StartInspectionTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.InspectionTasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return false;

            if (task.State != TaskState.Created && task.State != TaskState.Assigned && task.State != TaskState.RequiresRework)
            {
                return true;
            }

            task.StartExecution();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _notificationService.NotifyTaskStatusChangedAsync(task.Id, task.State.ToString(), cancellationToken);

            return true;
        }
    }

    public record CompleteInspectionTaskCommand(Guid TaskId) : IRequest<bool>;

    public class CompleteInspectionTaskCommandHandler : IRequestHandler<CompleteInspectionTaskCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IInspectionNotificationService _notificationService;

        public CompleteInspectionTaskCommandHandler(CadastreDbContext dbContext, IInspectionNotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(CompleteInspectionTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.InspectionTasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return false;

            if (task.State == TaskState.Completed || task.State == TaskState.Verified)
            {
                return true;
            }

            task.Complete();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _notificationService.NotifyTaskStatusChangedAsync(task.Id, task.State.ToString(), cancellationToken);

            return true;
        }
    }

    public record ApproveInspectionTaskCommand(Guid TaskId) : IRequest<bool>;

    public class ApproveInspectionTaskCommandHandler : IRequestHandler<ApproveInspectionTaskCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IInspectionNotificationService _notificationService;
        private readonly IMediator _mediator;

        public ApproveInspectionTaskCommandHandler(CadastreDbContext dbContext, IInspectionNotificationService notificationService, IMediator mediator)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            _mediator = mediator;
        }

        public async Task<bool> Handle(ApproveInspectionTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.InspectionTasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return false;

            task.Verify();

            bool hasViolations = task.ViolationStatus != ViolationStatus.None;

            var observations = await _dbContext.InspectionObservations
                .Where(o => o.InspectionTaskId == task.Id)
                .OrderByDescending(o => o.ObservationDate)
                .ToListAsync(cancellationToken);

            string remarksJson = "{}";
            if (observations.Any())
            {
                remarksJson = observations.First().RemarksJson;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _notificationService.NotifyTaskStatusChangedAsync(task.Id, task.State.ToString(), cancellationToken);

            await _mediator.Publish(new InspectionVerifiedEvent(task.Id, task.TargetSpatialUnitId, hasViolations, remarksJson), cancellationToken);

            return true;
        }
    }

    public record SendTaskForReworkCommand(Guid TaskId, string Reason) : IRequest<bool>;

    public class SendTaskForReworkCommandHandler : IRequestHandler<SendTaskForReworkCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;
        private readonly IInspectionNotificationService _notificationService;

        public SendTaskForReworkCommandHandler(CadastreDbContext dbContext, IInspectionNotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(SendTaskForReworkCommand request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.InspectionTasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return false;

            task.SendForRework(request.Reason);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _notificationService.NotifyTaskStatusChangedAsync(task.Id, task.State.ToString(), cancellationToken);

            return true;
        }
    }

    public record SaveInspectionResultCommand(Guid TaskId, ViolationStatus Status, string Conclusion) : IRequest<bool>;

    public class SaveInspectionResultCommandHandler : IRequestHandler<SaveInspectionResultCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public SaveInspectionResultCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(SaveInspectionResultCommand request, CancellationToken cancellationToken)
        {
            var task = await _dbContext.InspectionTasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
            if (task == null) return false;

            task.UpdateInspectionData(request.Status, request.Conclusion);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}