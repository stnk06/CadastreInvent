using System.Threading;
using System.Threading.Tasks;
using CadastreInvent.Shared.Application.Auth;

namespace CadastreInvent.Shared.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}