using System.Threading.Tasks;

namespace CadastreInvent.Valuation.Application.Services
{
    public interface IMassAppraisalNotificationService
    {
        Task NotifyProgressAsync(int processed, int total, string status);
    }
}