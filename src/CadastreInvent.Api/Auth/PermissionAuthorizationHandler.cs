using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CadastreInvent.Infrastructure.Persistence;

namespace CadastreInvent.Api.Auth
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
        {
            var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return;

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CadastreDbContext>();

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id.ToString() == userIdClaim);
            if (user == null || !user.IsActive) return;

            var role = await dbContext.Roles
                .Include(r => r.Permissions)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == user.RoleId);

            if (role == null) return;

            if (role.Permissions.Any(p => p.Permission == requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}