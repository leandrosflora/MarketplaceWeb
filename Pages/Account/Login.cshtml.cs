using System.ComponentModel.DataAnnotations;
using Marketplace.Web.Clients;
using Marketplace.Web.Infrastructure.Auth;
using Marketplace.Web.Infrastructure.Cart;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly IMarketplaceCredentialValidator _credentialValidator;
    private readonly IMarketplaceBffClient _bffClient;
    private readonly ICartOwnerIdAccessor _cartOwnerIdAccessor;

    public LoginModel(
        IMarketplaceCredentialValidator credentialValidator,
        IMarketplaceBffClient bffClient,
        ICartOwnerIdAccessor cartOwnerIdAccessor)
    {
        _credentialValidator = credentialValidator;
        _bffClient = bffClient;
        _cartOwnerIdAccessor = cartOwnerIdAccessor;
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

        var anonymousCartOwnerId = _cartOwnerIdAccessor.TryGetAnonymousCartOwnerId(HttpContext);
        var buyerIdClaim = principal.FindFirst(MarketplaceAuthConstants.BuyerIdClaim)?.Value;

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        if (anonymousCartOwnerId is not null && Guid.TryParse(buyerIdClaim, out var buyerId))
        {
            try
            {
                await _bffClient.MergeCartsAsync(anonymousCartOwnerId, buyerId.ToString(), HttpContext.RequestAborted);
            }
            catch (BffApiException)
            {
                // Best-effort merge: a failure here shouldn't block sign-in; the buyer's
                // anonymous cart items simply won't be carried over this time.
            }

            _cartOwnerIdAccessor.ClearAnonymousCartCookie(HttpContext);
        }

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
