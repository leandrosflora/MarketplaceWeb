using Marketplace.Web.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace MarketplaceWeb.Tests.Auth;

public sealed class MarketplaceCredentialValidatorTests
{
    [Fact]
    public void Validate_WithCustomerCredentials_ReturnsBuyerClaims()
    {
        var sut = new MarketplaceCredentialValidator(BuildOptions());

        var principal = sut.Validate("comprador", "comprador123");

        Assert.NotNull(principal);
        Assert.Equal("Comprador Demo", principal.Identity?.Name);
        Assert.Equal(
            "11111111-1111-1111-1111-111111111111",
            principal.FindFirst(MarketplaceAuthConstants.BuyerIdClaim)?.Value);
        Assert.Empty(principal.FindAll(claim => claim.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Validate_WithAdminCredentials_ReturnsAdminRoleClaims()
    {
        var sut = new MarketplaceCredentialValidator(BuildOptions());

        var principal = sut.Validate("admin", "admin123");

        Assert.NotNull(principal);
        Assert.True(principal.IsInRole("ADMIN_OPERATIONS"));
    }

    [Fact]
    public void Validate_WithInvalidPassword_ReturnsNull()
    {
        var sut = new MarketplaceCredentialValidator(BuildOptions());

        var principal = sut.Validate("admin", "wrong");

        Assert.Null(principal);
    }

    private static IOptionsMonitor<MarketplaceAuthOptions> BuildOptions()
    {
        var options = new MarketplaceAuthOptions
        {
            Users =
            [
                new MarketplaceAuthUserOptions
                {
                    Username = "comprador",
                    Password = "comprador123",
                    DisplayName = "Comprador Demo",
                    UserId = "11111111-1111-1111-1111-111111111111",
                    BuyerId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                },
                new MarketplaceAuthUserOptions
                {
                    Username = "admin",
                    Password = "admin123",
                    DisplayName = "Admin Demo",
                    UserId = "dev-operator@local",
                    Roles = ["ADMIN_OPERATIONS"]
                }
            ]
        };

        return new StaticOptionsMonitor<MarketplaceAuthOptions>(options);
    }
}
