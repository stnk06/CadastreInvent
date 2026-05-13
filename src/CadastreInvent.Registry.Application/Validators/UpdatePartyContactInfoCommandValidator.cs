using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class UpdatePartyContactInfoCommandValidator : AbstractValidator<UpdatePartyContactInfoCommand>
    {
        public UpdatePartyContactInfoCommandValidator()
        {
            RuleFor(x => x.PartyId).NotEmpty();
            RuleFor(x => x.NewContactInfo).NotEmpty().MaximumLength(500);
        }
    }
}