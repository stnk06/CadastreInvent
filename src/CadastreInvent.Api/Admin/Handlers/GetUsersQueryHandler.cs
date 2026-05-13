using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CadastreInvent.Infrastructure.Persistence;
using CadastreInvent.Shared.Application.Admin.DTOs;
using CadastreInvent.Shared.Application.Admin.Queries;

namespace CadastreInvent.Api.Admin.Handlers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
    {
        private readonly CadastreDbContext _dbContext;

        public GetUsersQueryHandler(CadastreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = from u in _dbContext.Users
                        join r in _dbContext.Roles on u.RoleId equals r.Id into roleGroup
                        from role in roleGroup.DefaultIfEmpty()
                        select new UserDto
                        {
                            Id = u.Id,
                            Username = u.Username,
                            Email = u.Email,
                            RoleId = u.RoleId,
                            RoleName = role != null ? role.Name : "Без роли",
                            IsActive = u.IsActive
                        };

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}