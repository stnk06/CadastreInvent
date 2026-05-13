using NetTopologySuite.Geometries;

namespace CadastreInvent.Shared.Application.Interfaces
{
    public interface ICoordinateTransformationService
    {
        /// <summary>
        /// Преобразует геометрию из локальной системы координат (МСК) в WGS84 (EPSG:4326) для отображения на web-картах.
        /// </summary>
        Geometry TransformToWgs84(Geometry geometry);

        /// <summary>
        /// Преобразует геометрию из WGS84 в целевую локальную систему координат.
        /// </summary>
        Geometry TransformFromWgs84(Geometry geometry, int targetSrid);
    }
}