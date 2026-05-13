using System;
using NetTopologySuite.Geometries;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Inspection.Domain.Enums;

namespace CadastreInvent.Inspection.Domain.Entities
{
    public class InspectionTask : DomainEntity
    {
        public Guid TargetSpatialUnitId { get; private set; }
        public Point TargetCoordinates { get; private set; }
        public string Description { get; private set; }
        public DateTime Deadline { get; private set; }
        public Guid? AssignedInspectorId { get; private set; }
        public TaskState State { get; private set; }
        public string? RejectionReason { get; private set; }

        public ViolationStatus ViolationStatus { get; private set; }
        public string? Conclusion { get; private set; }
        public Point? RecordedCoordinates { get; private set; }
        public bool HasGpsDiscrepancy { get; private set; }

        protected InspectionTask() { }

        public InspectionTask(Guid targetSpatialUnitId, Point targetCoordinates, string description, DateTime deadline)
        {
            if (targetSpatialUnitId == Guid.Empty) throw new ArgumentException(nameof(targetSpatialUnitId));
            if (targetCoordinates == null) throw new ArgumentNullException(nameof(targetCoordinates));
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentNullException(nameof(description));

            Id = Guid.NewGuid();
            TargetSpatialUnitId = targetSpatialUnitId;
            TargetCoordinates = targetCoordinates;
            TargetCoordinates.SRID = 4326;
            Description = description;
            Deadline = deadline;
            State = TaskState.Created;

            ViolationStatus = ViolationStatus.None;
            HasGpsDiscrepancy = false;

            CreatedAt = DateTime.UtcNow;
        }

        public void AssignTo(Guid inspectorId)
        {
            if (inspectorId == Guid.Empty) throw new ArgumentException(nameof(inspectorId));
            AssignedInspectorId = inspectorId;
            State = TaskState.Assigned;
            UpdateTimestamp();
        }

        public void StartExecution()
        {
            State = TaskState.InProgress;
            UpdateTimestamp();
        }

        public void Complete()
        {
            State = TaskState.Completed;
            UpdateTimestamp();
        }

        public void Verify()
        {
            State = TaskState.Verified;
            UpdateTimestamp();
        }

        public void SendForRework(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentNullException(nameof(reason));
            State = TaskState.InProgress;
            RejectionReason = reason; 
            UpdateTimestamp();
        }

        public void UpdateInspectionData(ViolationStatus status, string conclusion)
        {
            if (string.IsNullOrWhiteSpace(conclusion)) throw new ArgumentNullException(nameof(conclusion));
            ViolationStatus = status;
            Conclusion = conclusion;
            UpdateTimestamp();
        }

        public void SetRecordedCoordinates(Point coordinates)
        {
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            RecordedCoordinates = coordinates;
            RecordedCoordinates.SRID = 4326;
            UpdateTimestamp();
        }

        public void SetGpsDiscrepancy(bool hasDiscrepancy)
        {
            HasGpsDiscrepancy = hasDiscrepancy;
            UpdateTimestamp();
        }
    }
}