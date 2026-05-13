using Microsoft.AspNetCore.Authorization;

namespace CadastreInvent.Api.Auth
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base(policy: permission)
        {
        }
    }
}