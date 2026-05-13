using Microsoft.AspNetCore.Authorization;

namespace CadastreInvent.Api.Auth
{
    public class PermissionAuthorizationRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionAuthorizationRequirement(string permission)
        {
            Permission = permission;
        }
    }
}