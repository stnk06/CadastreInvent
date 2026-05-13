using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;
using CadastreInvent.Valuation.Domain.Enums;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class UpdateValuationAppealStatusCommandValidator : AbstractValidator<UpdateValuationAppealStatusCommand>
    {
        public UpdateValuationAppealStatusCommandValidator()
        {
            RuleFor(x => x.AppealId).NotEmpty();
            RuleFor(x => x.NewStatus).IsInEnum();

            RuleFor(x => x.NewAssessedValue)
                .NotNull()
                .GreaterThan(0)
                .When(x => x.NewStatus == AppealStatus.Resolved)
                .WithMessage("При удовлетворении апелляции (Resolved) необходимо указать новую пересчитанную стоимость участка.");
        }
    }
}