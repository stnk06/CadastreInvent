using System;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class ValuationAppealDto
    {
        public Guid Id { get; set; }
        public Guid ValuationId { get; set; }
        public Guid ApplicantPartyId { get; set; }
        public AppealStatus Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
    }
}