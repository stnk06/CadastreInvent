using CadastreInvent.Valuation.Application.DTOs;
using CadastreInvent.Valuation.Domain.Entities;

namespace CadastreInvent.Valuation.Application.Mappers
{
    public static class MassAppraisalModelMappingExtensions
    {
        public static MassAppraisalModelDto ToDto(this MassAppraisalModel model)
        {
            if (model == null) return null!;

            return new MassAppraisalModelDto
            {
                Id = model.Id,
                Version = model.Version,
                Description = model.Description,
                Algorithm = model.Algorithm,
                TrainingDate = model.CreatedAt
            };
        }
    }
}