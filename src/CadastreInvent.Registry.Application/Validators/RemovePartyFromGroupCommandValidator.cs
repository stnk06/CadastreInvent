using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class RemovePartyFromGroupCommandValidator : AbstractValidator<RemovePartyFromGroupCommand>
    {
        public RemovePartyFromGroupCommandValidator()
        {
            RuleFor(x => x.PartyGroupId).NotEmpty();
            RuleFor(x => x.PartyId).NotEmpty();
        }
    }
}