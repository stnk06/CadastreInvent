using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Mappers
{
    public static class ValuationAppealMappingExtensions
    {
        public static ValuationAppealDto ToDto(this ValuationAppeal entity)
        {
            if (entity == null) return null;

            return new ValuationAppealDto
            {
                Id = entity.Id,
                ValuationId = entity.ValuationId,
                ApplicantPartyId = entity.ApplicantPartyId,
                Status = entity.Status,
                Reason = entity.Reason,
                SubmissionDate = entity.SubmissionDate
            };
        }
    }
}