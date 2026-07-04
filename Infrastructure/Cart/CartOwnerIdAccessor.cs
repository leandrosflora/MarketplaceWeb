using Marketplace.Web.Infrastructure.Auth;

namespace Marketplace.Web.Infrastructure.Cart;

public sealed class CartOwnerIdAccessor : ICartOwnerIdAccessor
{
    public const string AnonymousPrefix = "anon:";
    private const string CookieName = "mkt_cart_id";

    public string GetOrCreateCartOwnerId(HttpContext context)
    {
        var buyerId = TryGetBuyerId(context);

        if (buyerId is { } id)
        {
            return id.ToString();
        }

        var existingAnonymousId = context.Request.Cookies[CookieName];

        if (!string.IsNullOrWhiteSpace(existingAnonymousId))
        {
            return AnonymousPrefix + existingAnonymousId;
        }

        var newAnonymousId = Guid.NewGuid().ToString("N");

        context.Response.Cookies.Append(CookieName, newAnonymousId, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

        return AnonymousPrefix + newAnonymousId;
    }

    public string? TryGetAnonymousCartOwnerId(HttpContext context)
    {
        var existingAnonymousId = context.Request.Cookies[CookieName];

        return string.IsNullOrWhiteSpace(existingAnonymousId) ? null : AnonymousPrefix + existingAnonymousId;
    }

    public void ClearAnonymousCartCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(CookieName);
    }

    private static Guid? TryGetBuyerId(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var claimValue = context.User.FindFirst(MarketplaceAuthConstants.BuyerIdClaim)?.Value;

        return Guid.TryParse(claimValue, out var buyerId) ? buyerId : null;
    }
}
