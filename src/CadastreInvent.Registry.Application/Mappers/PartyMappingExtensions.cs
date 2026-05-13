using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Mappers
{
    public static class PartyMappingExtensions
    {
        public static PartyDto ToDto(this Party entity)
        {
            if (entity == null) return null;

            return new PartyDto
            {
                Id = entity.Id,
                ExtId = entity.ExtId,
                Name = entity.Name,
                Type = entity.Type,
                ContactInfo = entity.ContactInfo
            };
        }
    }
}