using MediatR;

namespace CadastreInvent.Registry.Application.Queries
{
    public record GetSpatialUnitsGeoJsonQuery(
        double MinLon,
        double MinLat,
        double MaxLon,
        double MaxLat) : IRequest<string>;
}