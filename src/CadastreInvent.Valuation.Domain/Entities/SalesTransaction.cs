using System;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Domain.Entities
{
    public class SalesTransaction : DomainEntity
    {
        public Guid ValuationUnitId { get; private set; }
        public decimal SalePrice { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public TransactionValidity Validity { get; private set; }

        protected SalesTransaction() { }

        public SalesTransaction(Guid valuationUnitId, decimal salePrice, DateTime transactionDate, TransactionValidity validity)
        {
            if (valuationUnitId == Guid.Empty) throw new ArgumentException(nameof(valuationUnitId));
            if (salePrice <= 0) throw new ArgumentException(nameof(salePrice));

            Id = Guid.NewGuid();
            ValuationUnitId = valuationUnitId;
            SalePrice = salePrice;
            TransactionDate = transactionDate;
            Validity = validity;
            CreatedAt = DateTime.UtcNow;
        }

        public void InvalidateTransaction(TransactionValidity newValidity)
        {
            if (newValidity == TransactionValidity.ValidMarket) throw new ArgumentException(nameof(newValidity));
            Validity = newValidity;
            UpdateTimestamp();
        }
    }
}