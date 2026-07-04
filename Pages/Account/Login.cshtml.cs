using System.ComponentModel.DataAnnotations;
using Marketplace.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly IMarketplaceCredentialValidator _credentialValidator;

    public LoginModel(IMarketplaceCredentialValidator credentialValidator)
    {
        _credentialValidator = credentialValidator;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(ReturnUrl);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var principal = _credentialValidator.Validate(Input.Username, Input.Password);

        if (principal is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario ou senha invalidos.");
            return Page();
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        return RedirectToLocal(ReturnUrl);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToPage("/Index");
    }

    public sealed class LoginInput
    {
        [Required(ErrorMessage = "Informe o usuario.")]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;
    }
}
