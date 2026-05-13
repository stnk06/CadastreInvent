using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CadastreInvent.Shared.Application.Auth;

namespace CadastreInvent.Api.Pages
{
    [Authorize(Policy = Permissions.ManageBAUnits)]
    public class ImportModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}