using System;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace CadastreInvent.Registry.Application.Services
{
    public interface ISpatialValidationService
    {
        Task<string> GetTopologyErrorMessageAsync(Polygon polygon, Guid? excludeId, CancellationToken cancellationToken);
    }
}