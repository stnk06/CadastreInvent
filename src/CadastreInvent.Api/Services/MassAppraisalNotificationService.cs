using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using CadastreInvent.Api.Hubs;
using CadastreInvent.Valuation.Application.Services;

namespace CadastreInvent.Api.Services
{
    public class MassAppraisalNotificationService : IMassAppraisalNotificationService
    {
        private readonly IHubContext<MassAppraisalHub> _hubContext;

        public MassAppraisalNotificationService(IHubContext<MassAppraisalHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyProgressAsync(int processed, int total, string status)
        {
            await _hubContext.Clients.All.SendAsync("AppraisalProgress", new { status, processed, total });
        }
    }
}