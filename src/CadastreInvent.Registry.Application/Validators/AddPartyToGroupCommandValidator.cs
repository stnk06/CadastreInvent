using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class AddPartyToGroupCommandValidator : AbstractValidator<AddPartyToGroupCommand>
    {
        public AddPartyToGroupCommandValidator()
        {
            RuleFor(x => x.PartyGroupId).NotEmpty();
            RuleFor(x => x.PartyId).NotEmpty();

            RuleFor(x => x.ShareDenominator)
                .GreaterThan(0)
                .WithMessage("Знаменатель доли должен быть больше 0.");

            RuleFor(x => x.ShareNumerator)
                .GreaterThan(0)
                .LessThanOrEqualTo(x => x.ShareDenominator)
                .WithMessage("Числитель доли должен быть больше 0 и не превышать знаменатель.");
        }
    }
}