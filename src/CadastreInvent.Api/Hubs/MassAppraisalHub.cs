using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CadastreInvent.Api.Hubs
{
    [Authorize]
    public class MassAppraisalHub : Hub
    {
    }
}