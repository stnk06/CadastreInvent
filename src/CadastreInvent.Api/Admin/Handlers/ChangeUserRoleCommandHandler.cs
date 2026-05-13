using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Admin.Commands;

namespace CadastreInvent.Api.Admin.Handlers
{
    public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public ChangeUserRoleCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null) throw new Exception($"Пользователь с ID {request.UserId} не найден.");

            user.ChangeRole(request.RoleId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}