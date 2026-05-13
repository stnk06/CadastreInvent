using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Shared.Application.Interfaces;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Auth;

namespace CadastreInvent.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly CadastreDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public AuthService(CadastreDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Неверный логин или пароль");

            if (!user.IsActive)
                throw new Exception("Учетная запись заблокирована администратором системы");

            var role = await _dbContext.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);

            if (role == null)
                throw new Exception("Должностная роль не найдена");

            var token = _tokenService.GenerateToken(user, role);
            var refreshToken = _tokenService.GenerateRefreshToken();

            return new AuthResponse(token, refreshToken, user.Id, user.Username, user.Email, role.Name);
        }
    }
}