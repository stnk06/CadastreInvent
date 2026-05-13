using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class CreateBAUnitCommandValidator : AbstractValidator<CreateBAUnitCommand>
    {
        public CreateBAUnitCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255)
                .WithMessage("Название объекта права не может быть пустым.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Указан некорректный тип объекта права.");
        }
    }
}