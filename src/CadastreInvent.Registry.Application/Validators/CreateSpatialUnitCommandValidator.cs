using System;
using System.Threading;
using FluentValidation;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Registry.Application.Services;

namespace CadastreInvent.Registry.Application.Validators
{
    public class CreateSpatialUnitCommandValidator : AbstractValidator<CreateSpatialUnitCommand>
    {
        private readonly ISpatialValidationService _spatialValidationService;

        public CreateSpatialUnitCommandValidator(ISpatialValidationService spatialValidationService)
        {
            _spatialValidationService = spatialValidationService;

            RuleFor(x => x.ReferenceNumber)
                .NotEmpty()
                .Matches(@"^\d{2}:\d{2}:\d{6,7}:\d{1,10}$")
                .WithMessage("Неверный формат кадастрового номера. Ожидается формат XX:XX:XXXXXXX:XXXX.")
                .MaximumLength(100);

            RuleFor(x => x.Type)
                .IsInEnum();

            RuleFor(x => x.BoundaryWkt)
                .NotEmpty()
                .CustomAsync(async (wkt, context, cancellationToken) =>
                {
                    var polygon = GetPolygon(wkt);
                    if (polygon == null || !polygon.IsValid)
                    {
                        context.AddFailure("Некорректный формат WKT полигона.");
                        return;
                    }

                    var error = await _spatialValidationService.GetTopologyErrorMessageAsync(polygon, null, cancellationToken);
                    if (!string.IsNullOrEmpty(error))
                    {
                        context.AddFailure(error);
                    }
                });
        }

        private static Polygon? GetPolygon(string wkt)
        {
            if (string.IsNullOrWhiteSpace(wkt)) return null;

            try
            {
                var reader = new WKTReader();
                var geometry = reader.Read(wkt);

                if (geometry is Polygon polygon)
                {
                    polygon.SRID = 4326;
                    return polygon;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}