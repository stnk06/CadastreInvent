using System;
using System.Collections.Concurrent;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Infrastructure.Services
{
    public class CoordinateTransformationService : ICoordinateTransformationService
    {
        private readonly CoordinateTransformationFactory _ctFactory;
        private readonly CoordinateSystemFactory _csFactory;

        // Кэш для систем координат, чтобы не парсить WKT-строки каждый раз
        private readonly ConcurrentDictionary<int, CoordinateSystem> _csCache = new();
        private readonly CoordinateSystem _wgs84;

        public CoordinateTransformationService()
        {
            _ctFactory = new CoordinateTransformationFactory();
            _csFactory = new CoordinateSystemFactory();
            _wgs84 = GeographicCoordinateSystem.WGS84;

            _csCache.TryAdd(4326, _wgs84);
        }

        public Geometry TransformToWgs84(Geometry geometry)
        {
            if (geometry == null) return null!;
            if (geometry.SRID == 4326 || geometry.SRID == 0) return geometry;

            var sourceCs = GetCoordinateSystem(geometry.SRID);
            var trans = _ctFactory.CreateFromCoordinateSystems(sourceCs, _wgs84);

            var transformedGeometry = geometry.Copy();
            transformedGeometry.Apply(new MathTransformFilter(trans.MathTransform));
            transformedGeometry.SRID = 4326;

            return transformedGeometry;
        }

        public Geometry TransformFromWgs84(Geometry geometry, int targetSrid)
        {
            if (geometry == null) return null!;
            if (targetSrid == 4326 || targetSrid == 0) return geometry;

            var targetCs = GetCoordinateSystem(targetSrid);
            var trans = _ctFactory.CreateFromCoordinateSystems(_wgs84, targetCs);

            var transformedGeometry = geometry.Copy();
            transformedGeometry.Apply(new MathTransformFilter(trans.MathTransform));
            transformedGeometry.SRID = targetSrid;

            return transformedGeometry;
        }

        private CoordinateSystem GetCoordinateSystem(int srid)
        {
            return _csCache.GetOrAdd(srid, id =>
            {
                // В реальном Enterprise-приложении здесь будет запрос к БД со справочником пространственных систем (spatial_ref_sys).
                // Для примера захардкодим популярную в СНГ проекцию СК-42 / Гаусс-Крюгер зона 8 (SRID 28408).
                string wkt = string.Empty;

                if (id == 28408)
                {
                    wkt = @"PROJCS[""Pulkovo 1942 / Gauss-Kruger zone 8"",GEOGCS[""Pulkovo 1942"",DATUM[""Pulkovo_1942"",SPHEROID[""Krassowsky 1940"",6378245,298.3,AUTHORITY[""EPSG"",""7024""]],TOWGS84[23.92,-141.27,-80.9,-0,0.35,0.82,-0.12],AUTHORITY[""EPSG"",""6284""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4284""]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",0],PARAMETER[""central_meridian"",45],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",8500000],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AUTHORITY[""EPSG"",""28408""]]";
                }
                else
                {
                    throw new NotSupportedException($"Система координат со SRID {id} не зарегистрирована в справочнике.");
                }

                return _csFactory.CreateFromWkt(wkt);
            });
        }

        // Вспомогательный класс для применения трансформации к каждой точке полигона
        private class MathTransformFilter : ICoordinateSequenceFilter
        {
            // Исправлено: IMathTransform -> MathTransform
            private readonly MathTransform _transform;

            // Исправлено: IMathTransform -> MathTransform
            public MathTransformFilter(MathTransform transform) => _transform = transform;

            public bool Done => false;
            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence seq, int i)
            {
                var pt = new[] { seq.GetOrdinate(i, Ordinate.X), seq.GetOrdinate(i, Ordinate.Y) };
                var transformed = _transform.Transform(pt);
                seq.SetOrdinate(i, Ordinate.X, transformed[0]);
                seq.SetOrdinate(i, Ordinate.Y, transformed[1]);
            }
        }
    }
}