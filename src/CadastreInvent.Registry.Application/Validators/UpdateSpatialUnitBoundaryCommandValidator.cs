using System;
using System.Threading;
using FluentValidation;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using CadastreInvent.Registry.Application.Commands;
using CadastreInvent.Registry.Application.Services;

namespace CadastreInvent.Registry.Application.Validators
{
    public class UpdateSpatialUnitBoundaryCommandValidator : AbstractValidator<UpdateSpatialUnitBoundaryCommand>
    {
        private readonly ISpatialValidationService _spatialValidationService;

        public UpdateSpatialUnitBoundaryCommandValidator(ISpatialValidationService spatialValidationService)
        {
            _spatialValidationService = spatialValidationService;

            RuleFor(x => x.SpatialUnitId).NotEmpty();

            RuleFor(x => x.NewBoundaryWkt)
                .NotEmpty()
                .CustomAsync(async (wkt, context, cancellationToken) =>
                {
                    var polygon = GetPolygon(wkt);
                    if (polygon == null || !polygon.IsValid)
                    {
                        context.AddFailure("Некорректный формат WKT полигона.");
                        return;
                    }

                    var cmd = context.InstanceToValidate;
                    var error = await _spatialValidationService.GetTopologyErrorMessageAsync(polygon, cmd.SpatialUnitId, cancellationToken);

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