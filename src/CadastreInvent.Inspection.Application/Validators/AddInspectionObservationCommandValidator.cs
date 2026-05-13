using FluentValidation;
using CadastreInvent.Inspection.Application.Commands;

namespace CadastreInvent.Inspection.Application.Validators
{
    public class AddInspectionObservationCommandValidator : AbstractValidator<AddInspectionObservationCommand>
    {
        public AddInspectionObservationCommandValidator()
        {
            RuleFor(x => x.InspectionTaskId)
                .NotEmpty()
                .WithMessage("Идентификатор ордера инспекции обязателен.");

            RuleFor(x => x.Category)
                .IsInEnum()
                .WithMessage("Указана недопустимая категория наблюдения.");

            RuleFor(x => x.RemarksJson)
                .NotEmpty()
                .WithMessage("Заметки акта наблюдения не могут быть пустыми.");

            RuleFor(x => x.ObservationDate)
                .NotEmpty()
                .WithMessage("Дата наблюдения обязательна.");
        }
    }
}