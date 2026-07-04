namespace Marketplace.Web.Infrastructure.Auth;

public sealed class MarketplaceAuthOptions
{
    public const string SectionName = "MarketplaceAuth";

    public IReadOnlyList<MarketplaceAuthUserOptions> Users { get; init; } = [];

    public bool EnableDevAdminIdentityFallback { get; init; }
}

public sealed class MarketplaceAuthUserOptions
{
    public string Username { get; init; } = string.Empty;

    public string? Password { get; init; }

    public string? PasswordHash { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string UserId { get; init; } = string.Empty;

    public Guid? BuyerId { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];
}
