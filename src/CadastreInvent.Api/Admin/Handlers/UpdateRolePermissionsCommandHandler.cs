using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Admin.Commands;

namespace CadastreInvent.Api.Admin.Handlers
{
    public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public UpdateRolePermissionsCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            var role = await _dbContext.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null) throw new Exception($"Роль с кодом {request.RoleId} не найдена.");

            List<string> permissions;
            try
            {
                permissions = JsonSerializer.Deserialize<List<string>>(request.PermissionsJson) ?? new List<string>();
            }
            catch
            {
                throw new Exception("Некорректный формат JSON для списка прав (Permissions).");
            }

            role.ClearPermissions();

            foreach (var p in permissions)
            {
                role.AddPermission(p);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}