using FluentValidation;
using CadastreInvent.Registry.Application.Queries;

namespace CadastreInvent.Registry.Application.Validators
{
    public class GetSpatialUnitsGeoJsonQueryValidator : AbstractValidator<GetSpatialUnitsGeoJsonQuery>
    {
        public GetSpatialUnitsGeoJsonQueryValidator()
        {
            RuleFor(x => x.MinLon).InclusiveBetween(-180, 180);
            RuleFor(x => x.MaxLon).InclusiveBetween(-180, 180);
            RuleFor(x => x.MinLat).InclusiveBetween(-90, 90);
            RuleFor(x => x.MaxLat).InclusiveBetween(-90, 90);
            RuleFor(x => x.MinLon).LessThan(x => x.MaxLon);
            RuleFor(x => x.MinLat).LessThan(x => x.MaxLat);
        }
    }
}