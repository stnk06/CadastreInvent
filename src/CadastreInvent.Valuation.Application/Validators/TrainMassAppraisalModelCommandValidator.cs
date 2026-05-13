using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class TrainMassAppraisalModelCommandValidator : AbstractValidator<TrainMassAppraisalModelCommand>
    {
        public TrainMassAppraisalModelCommandValidator()
        {
            RuleFor(x => x.Version)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(1000);
        }
    }
}