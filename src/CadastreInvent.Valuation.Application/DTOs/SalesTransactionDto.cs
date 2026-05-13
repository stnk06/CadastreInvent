using System;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.DTOs
{
    public class SalesTransactionDto
    {
        public Guid Id { get; set; }
        public Guid ValuationUnitId { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionValidity Validity { get; set; }
    }
}