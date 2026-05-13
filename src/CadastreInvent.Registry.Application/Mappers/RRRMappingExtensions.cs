using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Mappers
{
    public static class RRRMappingExtensions
    {
        public static RRRDto ToDto(this RRR entity)
        {
            if (entity == null) return null;

            return new RRRDto
            {
                Id = entity.Id,
                Type = entity.Type,
                BAUnitId = entity.BAUnitId,
                PartyId = entity.PartyId,
                PartyGroupId = entity.PartyGroupId,
                SourceId = entity.SourceId,
                ShareNumerator = entity.ShareNumerator,
                ShareDenominator = entity.ShareDenominator,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            };
        }
    }
}