using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Marketplace.Web.Pages.Account;

public sealed class LoginModel : PageModel
{
    private static readonly TimeSpan IdentityProviderCheckTimeout = TimeSpan.FromSeconds(2);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<OpenIdConnectOptions> _openIdConnectOptions;

    public LoginModel(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions)
    {
        _httpClientFactory = httpClientFactory;
        _openIdConnectOptions = openIdConnectOptions;
    }

    public string ReturnUrl { get; private set; } = "/";

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet(string? returnUrl = "/")
    {
        ReturnUrl = NormalizeReturnUrl(returnUrl);

        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(ReturnUrl);
        }

        return Page();
    }

    public async Task<IActionResult> OnGetStartAsync(
        string? returnUrl = "/",
        CancellationToken cancellationToken = default)
    {
        ReturnUrl = NormalizeReturnUrl(returnUrl);

        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(ReturnUrl);
        }

        if (!await IsIdentityProviderAvailableAsync(cancellationToken))
        {
            ErrorMessage = "Não foi possível conectar ao provedor de identidade. "
                + "Verifique se o serviço de autenticação está em execução e se a configuração OpenIdConnect:Authority está correta.";

            return Page();
        }

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = ReturnUrl
            },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    private async Task<bool> IsIdentityProviderAvailableAsync(CancellationToken cancellationToken)
    {
        var options = _openIdConnectOptions.Get(OpenIdConnectDefaults.AuthenticationScheme);

        if (string.IsNullOrWhiteSpace(options.Authority)
            || !Uri.TryCreate(options.Authority, UriKind.Absolute, out var authorityUri))
        {
            return false;
        }

        var discoveryUri = new Uri(authorityUri, "/.well-known/openid-configuration");
        var client = _httpClientFactory.CreateClient();
        client.Timeout = IdentityProviderCheckTimeout;

        try
        {
            using var response = await client.GetAsync(discoveryUri, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private string NormalizeReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : "/";
    }
}
