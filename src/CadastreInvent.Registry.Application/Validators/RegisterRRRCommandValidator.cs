using FluentValidation;
using CadastreInvent.Registry.Application.Commands;
using System;

namespace CadastreInvent.Registry.Application.Validators
{
    public class RegisterRRRCommandValidator : AbstractValidator<RegisterRRRCommand>
    {
        public RegisterRRRCommandValidator()
        {
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.BAUnitId).NotEmpty();
            RuleFor(x => x.SourceId).NotEmpty();

            RuleFor(x => x)
                .Must(x => (x.PartyId.HasValue && x.PartyId != Guid.Empty) ^ (x.PartyGroupId.HasValue && x.PartyGroupId != Guid.Empty))
                .WithMessage("Необходимо указать либо индивидуального субъекта, либо группу субъектов.");

            RuleFor(x => x.ShareDenominator)
                .GreaterThan(0)
                .WithMessage("Знаменатель доли должен быть больше 0.");

            RuleFor(x => x.ShareNumerator)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(x => x.ShareDenominator)
                .WithMessage("Числитель доли не может быть меньше 0 или больше знаменателя.");

            RuleFor(x => x.StartDate).NotEmpty();
        }
    }
}