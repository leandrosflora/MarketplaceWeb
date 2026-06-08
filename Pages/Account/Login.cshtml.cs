using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Account;

public sealed class LoginModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = "/")
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
            },
            OpenIdConnectDefaults.AuthenticationScheme);
    }
}
