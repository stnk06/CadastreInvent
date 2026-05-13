using FluentValidation;
using CadastreInvent.Valuation.Application.Commands;

namespace CadastreInvent.Valuation.Application.Validators
{
    public class AddPropertyCharacteristicCommandValidator : AbstractValidator<AddPropertyCharacteristicCommand>
    {
        public AddPropertyCharacteristicCommandValidator()
        {
            RuleFor(x => x.ValuationUnitId)
                .NotEmpty();

            RuleFor(x => x.CharacteristicsJson)
                .NotEmpty()
                .WithMessage("JSON с характеристиками объекта не может быть пустым.");
        }
    }
}