using System;

namespace CadastreInvent.Shared.Application.Auth
{
    public record AuthResponse(string Token, string RefreshToken, Guid UserId, string Username, string Email, string Role);
}