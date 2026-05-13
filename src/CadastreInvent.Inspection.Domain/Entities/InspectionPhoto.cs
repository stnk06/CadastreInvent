using System;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Inspection.Domain.Entities
{
    public class InspectionPhoto : DomainEntity
    {
        public Guid InspectionTaskId { get; private set; }
        public string FilePath { get; private set; }
        public Guid? AppLocalId { get; private set; }

        protected InspectionPhoto() { }

        public InspectionPhoto(Guid inspectionTaskId, string filePath, Guid? appLocalId = null)
        {
            if (inspectionTaskId == Guid.Empty) throw new ArgumentException(nameof(inspectionTaskId));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            Id = Guid.NewGuid();
            InspectionTaskId = inspectionTaskId;
            FilePath = filePath;
            AppLocalId = appLocalId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}