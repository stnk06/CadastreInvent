using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CadastreInvent.Infrastructure.Persistence; 
using CadastreInvent.Shared.Application.Admin.Commands;

namespace CadastreInvent.Api.Admin.Handlers
{
    public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, bool>
    {
        private readonly CadastreDbContext _dbContext;

        public ChangeUserPasswordCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null) throw new Exception($"Сотрудник с кодом {request.UserId} не найден.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ChangePassword(passwordHash);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}