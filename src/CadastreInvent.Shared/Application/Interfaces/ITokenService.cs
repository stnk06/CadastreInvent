using CadastreInvent.Shared.Domain.Entities;

namespace CadastreInvent.Shared.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, Role role);
        string GenerateRefreshToken();
    }
}