using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;
using System;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class RegisterSalesTransactionCommandValidator : AbstractValidator<RegisterSalesTransactionCommand>
    {
        public RegisterSalesTransactionCommandValidator()
        {
            RuleFor(x => x.ValuationUnitId).NotEmpty();

            RuleFor(x => x.SalePrice)
                .GreaterThan(0)
                .WithMessage("Цена сделки должна быть больше нуля.");

            RuleFor(x => x.TransactionDate)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Дата сделки не может быть в будущем.");
        }
    }
}