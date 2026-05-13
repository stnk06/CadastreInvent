using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Admin.Commands;

namespace CadastreInvent.Api.Admin.Handlers
{
    public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public ToggleUserStatusCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null) throw new Exception($"Пользователь с ID {request.UserId} не найден.");

            if (request.Activate)
                user.Activate();
            else
                user.Deactivate();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}