using CadastreInvent.Registry.Domain.Entities;
using CadastreInvent.Registry.Application.DTOs;

namespace CadastreInvent.Registry.Application.Mappers
{
    public static class SourceMappingExtensions
    {
        public static SourceDto ToDto(this Source entity)
        {
            if (entity == null) return null;

            return new SourceDto
            {
                Id = entity.Id,
                Type = entity.Type,
                DocumentNumber = entity.DocumentNumber,
                RecordDate = entity.RecordDate,
                ContentUrl = entity.ContentUrl
            };
        }
    }
}