namespace Marketplace.Web.Infrastructure;

/// <summary>
/// Interim stand-in for a real logged-in admin identity: attaches a fixed operator id and role
/// set to every call to MarketplaceAdmin.Bff, read from configuration. MarketplaceWeb has no
/// authentication system yet (see openspec/changes/admin-backoffice/design.md, Open Questions),
/// so there is no session to derive a real caller/roles from. Replace this handler once real
/// admin login exists; the Admin BFF only depends on the X-Admin-User-Id/X-Admin-Roles headers,
/// so nothing downstream needs to change when it does.
/// </summary>
public sealed class DevAdminIdentityHandler : DelegatingHandler
{
    private readonly IConfiguration _configuration;

    public DevAdminIdentityHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var userId = _configuration["AdminBff:DevIdentity:UserId"];
        var roles = _configuration["AdminBff:DevIdentity:Roles"];

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
