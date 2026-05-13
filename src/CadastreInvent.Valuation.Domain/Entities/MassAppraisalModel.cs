using System;
using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Valuation.Domain.Entities
{
    public class MassAppraisalModel : DomainEntity
    {
        public string Version { get; private set; }
        public string Description { get; private set; }
        public string Algorithm { get; private set; }
        public string Status { get; private set; }

        public byte[] ModelData { get; private set; }
        public string MetricsJson { get; private set; }

        public DateTime TrainingDate { get; private set; }

        protected MassAppraisalModel()
        {
            Version = string.Empty;
            Description = string.Empty;
            Algorithm = string.Empty;
            Status = "Pending";
            ModelData = Array.Empty<byte>();
            MetricsJson = string.Empty;
        }

        public MassAppraisalModel(string version, string description, string algorithm, DateTime createdAt)
        {
            Id = Guid.NewGuid();
            Version = version;
            Description = description;
            Algorithm = algorithm;
            Status = "Pending";
            CreatedAt = createdAt;
            TrainingDate = createdAt;
            ModelData = Array.Empty<byte>();
            MetricsJson = string.Empty;
        }

        public void SetTrainedModel(byte[] modelData, string metricsJson, string status)
        {
            ModelData = modelData ?? Array.Empty<byte>();
            MetricsJson = string.IsNullOrWhiteSpace(metricsJson) ? "{}" : metricsJson;
            Status = status;
            UpdateTimestamp();
        }
    }
}