using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class ExecuteMassAppraisalCommandValidator : AbstractValidator<ExecuteMassAppraisalCommand>
    {
        public ExecuteMassAppraisalCommandValidator()
        {
            RuleFor(x => x.ModelId)
                .NotEmpty()
                .WithMessage("Идентификатор ML-модели обязателен для запуска массовой оценки.");

            When(x => x.MinArea.HasValue, () =>
            {
                RuleFor(x => x.MinArea)
                    .GreaterThan(0)
                    .WithMessage("Минимальная площадь должна быть больше нуля.");
            });

            When(x => x.MaxArea.HasValue, () =>
            {
                RuleFor(x => x.MaxArea)
                    .GreaterThan(0)
                    .WithMessage("Максимальная площадь должна быть больше нуля.");
            });

            When(x => x.MinArea.HasValue && x.MaxArea.HasValue, () =>
            {
                RuleFor(x => x.MaxArea)
                    .GreaterThan(x => x.MinArea)
                    .WithMessage("Максимальный порог площади должен быть больше минимального.");
            });
        }
    }
}