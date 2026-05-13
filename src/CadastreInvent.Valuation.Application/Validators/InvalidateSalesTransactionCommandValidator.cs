using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class InvalidateSalesTransactionCommandValidator : AbstractValidator<InvalidateSalesTransactionCommand>
    {
        public InvalidateSalesTransactionCommandValidator()
        {
            RuleFor(x => x.TransactionId).NotEmpty();

            RuleFor(x => x.NewValidity)
                .IsInEnum()
                .NotEqual(TransactionValidity.ValidMarket)
                .WithMessage("Для инвалидации необходимо указать причину невалидности (например, InvalidAffiliated).");
        }
    }
}