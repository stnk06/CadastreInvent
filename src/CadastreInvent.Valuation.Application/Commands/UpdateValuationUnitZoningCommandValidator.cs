using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class UpdateValuationUnitZoningCommandValidator : AbstractValidator<UpdateValuationUnitZoningCommand>
    {
        public UpdateValuationUnitZoningCommandValidator()
        {
            RuleFor(x => x.ValuationUnitId).NotEmpty();
            RuleFor(x => x.ZoningStatus).NotEmpty().MaximumLength(100);
        }
    }
}