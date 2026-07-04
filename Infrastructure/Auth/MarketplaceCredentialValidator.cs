using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Marketplace.Web.Infrastructure.Auth;

public interface IMarketplaceCredentialValidator
{
    ClaimsPrincipal? Validate(string username, string password);
}

public sealed class MarketplaceCredentialValidator : IMarketplaceCredentialValidator
{
    private readonly IOptionsMonitor<MarketplaceAuthOptions> _options;
    private readonly PasswordHasher<MarketplaceAuthUserOptions> _passwordHasher = new();

    public MarketplaceCredentialValidator(IOptionsMonitor<MarketplaceAuthOptions> options)
    {
        _options = options;
    }

    public ClaimsPrincipal? Validate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var user = _options.CurrentValue.Users.FirstOrDefault(candidate =>
            string.Equals(candidate.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null || !IsPasswordValid(user, password))
        {
            return null;
        }

        var userId = string.IsNullOrWhiteSpace(user.UserId) ? user.Username : user.UserId;
        var displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Username : user.DisplayName;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, displayName)
        };

        if (user.BuyerId is { } buyerId)
        {
            claims.Add(new Claim(MarketplaceAuthConstants.BuyerIdClaim, buyerId.ToString()));
        }

        foreach (var role in user.Roles.Where(role => !string.IsNullOrWhiteSpace(role)))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    private bool IsPasswordValid(MarketplaceAuthUserOptions user, string password)
    {
        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }

        return !string.IsNullOrEmpty(user.Password)
            && string.Equals(user.Password, password, StringComparison.Ordinal);
    }
}
