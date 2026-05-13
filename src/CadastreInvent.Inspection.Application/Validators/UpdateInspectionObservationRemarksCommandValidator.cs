using FluentValidation;
using CadastreInvent.Inspection.Application.Commands;

namespace CadastreInvent.Inspection.Application.Validators
{
    public class UpdateInspectionObservationRemarksCommandValidator : AbstractValidator<UpdateInspectionObservationRemarksCommand>
    {
        public UpdateInspectionObservationRemarksCommandValidator()
        {
            RuleFor(x => x.ObservationId).NotEmpty();
            RuleFor(x => x.RemarksJson).NotEmpty();
        }
    }
}