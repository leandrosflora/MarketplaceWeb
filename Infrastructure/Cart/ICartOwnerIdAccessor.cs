namespace Marketplace.Web.Infrastructure.Cart;

/// <summary>
/// Resolves the opaque `cartOwnerId` string sent to `MarketplaceWeb.Bff` on every cart call:
/// the authenticated buyer's `BuyerId` claim value, or an anonymous cart id (cookie-issued GUID
/// prefixed with "anon:") when not logged in. The prefix lets the BFF tell the two apart without
/// needing its own session/auth state — see `CartOwnerIdHelper` on the BFF side.
/// </summary>
public interface ICartOwnerIdAccessor
{
    string GetOrCreateCartOwnerId(HttpContext context);

    string? TryGetAnonymousCartOwnerId(HttpContext context);

    void ClearAnonymousCartCookie(HttpContext context);
}
