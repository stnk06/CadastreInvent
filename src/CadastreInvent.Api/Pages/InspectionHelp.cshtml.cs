using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CadastreInvent.Api.Pages
{
    [Authorize]
    public class InspectionHelpModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}