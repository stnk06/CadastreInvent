using FluentValidation;
using CadastreInvent.Registry.Application.Commands;

namespace CadastreInvent.Registry.Application.Validators
{
    public class RemoveSpatialUnitFromBAUnitCommandValidator : AbstractValidator<RemoveSpatialUnitFromBAUnitCommand>
    {
        public RemoveSpatialUnitFromBAUnitCommandValidator()
        {
            RuleFor(x => x.BAUnitId).NotEmpty();
            RuleFor(x => x.SpatialUnitId).NotEmpty();
        }
    }
}