using System;
using CadastreInvent.Shared.Application.Interfaces;

namespace CadastreInvent.Infrastructure.Services
{
    public class MockCurrentUserService : ICurrentUserService
    {
        public Guid UserId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}