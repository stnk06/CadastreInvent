using CadastreInvent.Valuation.Domain.Entities;
using CadastreInvent.Valuation.Application.DTOs;

namespace CadastreInvent.Valuation.Application.Mappers
{
    public static class SalesTransactionMappingExtensions
    {
        public static SalesTransactionDto ToDto(this SalesTransaction entity)
        {
            if (entity == null) return null;

            return new SalesTransactionDto
            {
                Id = entity.Id,
                ValuationUnitId = entity.ValuationUnitId,
                SalePrice = entity.SalePrice,
                TransactionDate = entity.TransactionDate,
                Validity = entity.Validity
            };
        }
    }
}