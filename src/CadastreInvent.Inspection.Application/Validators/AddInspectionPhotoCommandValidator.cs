using FluentValidation;
using CadastreInvent.Inspection.Application.Commands;

namespace CadastreInvent.Inspection.Application.Validators
{
    public class AddInspectionPhotoCommandValidator : AbstractValidator<AddInspectionPhotoCommand>
    {
        public AddInspectionPhotoCommandValidator()
        {
            RuleFor(x => x.InspectionTaskId)
                .NotEmpty()
                .WithMessage("Идентификатор ордера инспекции обязателен.");

            RuleFor(x => x.PhotoUrl)
                .NotEmpty()
                .WithMessage("Путь или URL к фотографии обязателен.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Недопустимое значение долготы. Должно быть в пределах от -180 до 180.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Недопустимое значение широты. Должно быть в пределах от -90 до 90.");

            RuleFor(x => x.Azimuth)
                .InclusiveBetween(0, 360)
                .WithMessage("Азимут должен быть в пределах от 0 до 360 градусов.");

            RuleFor(x => x.CaptureDate)
                .NotEmpty()
                .WithMessage("Дата и время съемки обязательны.");
        }
    }
}