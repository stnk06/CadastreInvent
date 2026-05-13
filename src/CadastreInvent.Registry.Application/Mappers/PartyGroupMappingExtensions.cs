using System.Linq;
using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Mappers
{
    public static class PartyGroupMappingExtensions
    {
        public static PartyGroupDto ToDto(this PartyGroup entity)
        {
            if (entity == null) return null;

            return new PartyGroupDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Members = entity.Members?.Select(m => new PartyGroupMemberDto
                {
                    PartyId = m.PartyId,
                    ShareNumerator = m.ShareNumerator,
                    ShareDenominator = m.ShareDenominator
                }).ToList() ?? new()
            };
        }
    }
}