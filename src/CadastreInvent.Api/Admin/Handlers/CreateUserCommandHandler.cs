using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Domain.Entities;
using CadastreInvent.Shared.Application.Admin.Commands;

namespace CadastreInvent.Api.Admin.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly CadastreDbContext _dbContext;

        public CreateUserCommandHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            bool exists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
            if (exists)
            {
                throw new Exception("Пользователь с таким Email уже существует.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User(request.Username, request.Email, passwordHash, request.RoleId);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}