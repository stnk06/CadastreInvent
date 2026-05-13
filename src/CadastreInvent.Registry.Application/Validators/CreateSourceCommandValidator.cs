using FluentValidation;
using CadastreInvent.Registry.Application.Commands;
using System;

namespace CadastreInvent.Registry.Application.Validators
{
    public class CreateSourceCommandValidator : AbstractValidator<CreateSourceCommand>
    {
        public CreateSourceCommandValidator()
        {
            RuleFor(x => x.Type).IsInEnum();

            RuleFor(x => x.DocumentNumber)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.RecordDate)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Дата регистрации документа не может быть в будущем.");

            RuleFor(x => x.ContentUrl)
                .MaximumLength(1000);
        }
    }
}