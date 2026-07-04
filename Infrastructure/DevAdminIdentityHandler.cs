using System.Security.Claims;
using Marketplace.Web.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Marketplace.Web.Infrastructure;

/// <summary>
/// Propagates the authenticated admin identity expected by MarketplaceAdmin.Bff.
/// </summary>
public sealed class DevAdminIdentityHandler : DelegatingHandler
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<MarketplaceAuthOptions> _authOptions;

    public DevAdminIdentityHandler(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<MarketplaceAuthOptions> authOptions)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _authOptions = authOptions;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var roles = principal is null
            ? null
            : string.Join(
                ',',
                principal.FindAll(ClaimTypes.Role)
                    .Select(claim => claim.Value)
                    .Where(role => MarketplaceAuthConstants.AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase));

        if ((string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roles))
            && _authOptions.CurrentValue.EnableDevAdminIdentityFallback)
        {
            userId = _configuration["AdminBff:DevIdentity:UserId"];
            roles = _configuration["AdminBff:DevIdentity:Roles"];
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            request.Headers.TryAddWithoutValidation("X-Admin-User-Id", userId);
        }

        if (!string.IsNullOrWhiteSpace(roles))
        {
            request.Headers.TryAddWithoutValidation("X-Admin-Roles", roles);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
