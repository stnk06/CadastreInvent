using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CadastreInvent.Shared.Application.Interfaces;
using CadastreInvent.Shared.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

namespace CadastreInvent.Api.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty(SupportsGet = true)]
        public bool SessionExpired { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Введите корпоративный Email")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Введите пароль доступа")]
        public string Password { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            if (User.Identity is { IsAuthenticated: true } && !SessionExpired)
            {
                return RedirectToPage("/Index");
            }

            if (SessionExpired)
            {
                Response.Cookies.Delete("AuthToken");
                ModelState.AddModelError(string.Empty, "Срок действия вашей сессии истек по соображениям безопасности. Пожалуйста, авторизуйтесь повторно.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _authService.LoginAsync(new LoginRequest(Email, Password), CancellationToken.None);

                Response.Cookies.Append("AuthToken", response.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(120)
                });

                return RedirectToPage("/Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Указаны некорректные учетные данные или доступ заблокирован.");
                return Page();
            }
        }

        public IActionResult OnPostLogout()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToPage("/Login");
        }

        public IActionResult OnPostEsiaLogin()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/Index" }, "ESIA");
        }
    }
}