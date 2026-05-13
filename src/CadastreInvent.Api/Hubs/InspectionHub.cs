using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CadastreInvent.Shared.Application.Auth;

namespace CadastreInvent.Api.Hubs
{
    [Authorize(Policy = Permissions.ManageFieldTasks)]
    public class InspectionHub : Hub
    {
    }
}