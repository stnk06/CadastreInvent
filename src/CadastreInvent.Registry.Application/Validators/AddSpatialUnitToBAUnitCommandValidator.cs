using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class AddSpatialUnitToBAUnitCommandValidator : AbstractValidator<AddSpatialUnitToBAUnitCommand>
    {
        public AddSpatialUnitToBAUnitCommandValidator()
        {
            RuleFor(x => x.BAUnitId).NotEmpty();
            RuleFor(x => x.SpatialUnitId).NotEmpty();
        }
    }
}