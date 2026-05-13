using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class CreatePartyGroupCommandValidator : AbstractValidator<CreatePartyGroupCommand>
    {
        public CreatePartyGroupCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255);
        }
    }
}