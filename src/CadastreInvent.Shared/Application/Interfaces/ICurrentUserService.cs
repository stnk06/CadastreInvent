using System;

namespace CadastreInvent.Shared.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
    }
}