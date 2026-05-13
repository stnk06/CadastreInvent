using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class CreateValuationAppealCommandValidator : AbstractValidator<CreateValuationAppealCommand>
    {
        public CreateValuationAppealCommandValidator()
        {
            RuleFor(x => x.ValuationId).NotEmpty();
            RuleFor(x => x.ApplicantPartyId).NotEmpty();
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MaximumLength(2000)
                .WithMessage("Причина апелляции обязательна и не должна превышать 2000 символов.");
        }
    }
}