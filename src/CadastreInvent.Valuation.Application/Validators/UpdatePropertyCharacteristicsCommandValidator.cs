using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class UpdatePropertyCharacteristicsCommandValidator : AbstractValidator<UpdatePropertyCharacteristicsCommand>
    {
        public UpdatePropertyCharacteristicsCommandValidator()
        {
            RuleFor(x => x.CharacteristicId).NotEmpty();
            RuleFor(x => x.CharacteristicsJson).NotEmpty();
        }
    }
}