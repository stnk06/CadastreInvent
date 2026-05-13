using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CadastreInvent.Infrastructure.Integration
{
    public interface IExternalCadastreService
    {
        Task<List<ExternalPropertyDto>> GetPropertiesInAreaAsync(string wktPolygon, CancellationToken cancellationToken);
    }
}