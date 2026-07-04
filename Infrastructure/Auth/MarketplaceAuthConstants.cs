namespace Marketplace.Web.Infrastructure.Auth;

public static class MarketplaceAuthConstants
{
    public const string AdminPolicy = "MarketplaceAdmin";
    public const string BuyerIdClaim = "buyer_id";

    public static readonly string[] AdminRoles =
    [
        "ADMIN_CATALOG",
        "ADMIN_INVENTORY",
        "ADMIN_LOGISTICS",
        "ADMIN_PRICING",
        "ADMIN_OPERATIONS",
        "ADMIN_READONLY"
    ];
}
