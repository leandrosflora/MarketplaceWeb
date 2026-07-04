using System.Net;
using System.Security.Claims;
using Marketplace.Web.Infrastructure;
using Marketplace.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MarketplaceWeb.Tests.Auth;

public sealed class AdminIdentityHandlerTests
{
    [Fact]
    public async Task SendAsync_WithAuthenticatedAdmin_AddsIdentityHeadersFromClaims()
    {
        var terminal = new RecordingHandler();
        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "admin-user"),
                    new Claim(ClaimTypes.Role, "ADMIN_OPERATIONS"),
                    new Claim(ClaimTypes.Role, "IGNORED")
                ], "Test"))
            }
        };
        var sut = new DevAdminIdentityHandler(
            new ConfigurationBuilder().Build(),
            contextAccessor,
            new StaticOptionsMonitor<MarketplaceAuthOptions>(new MarketplaceAuthOptions()))
        {
            InnerHandler = terminal
        };
        using var invoker = new HttpMessageInvoker(sut);

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://admin-bff.local/admin"), CancellationToken.None);

        Assert.Equal("admin-user", terminal.Request?.Headers.GetValues("X-Admin-User-Id").Single());
        Assert.Equal("ADMIN_OPERATIONS", terminal.Request?.Headers.GetValues("X-Admin-Roles").Single());
    }

    [Fact]
    public async Task SendAsync_WithoutPrincipal_UsesConfiguredFallbackOnlyWhenEnabled()
    {
        var terminal = new RecordingHandler();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminBff:DevIdentity:UserId"] = "dev-operator@local",
                ["AdminBff:DevIdentity:Roles"] = "ADMIN_OPERATIONS"
            })
            .Build();
        var sut = new DevAdminIdentityHandler(
            configuration,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            new StaticOptionsMonitor<MarketplaceAuthOptions>(
                new MarketplaceAuthOptions { EnableDevAdminIdentityFallback = true }))
        {
            InnerHandler = terminal
        };
        using var invoker = new HttpMessageInvoker(sut);

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://admin-bff.local/admin"), CancellationToken.None);

        Assert.Equal("dev-operator@local", terminal.Request?.Headers.GetValues("X-Admin-User-Id").Single());
        Assert.Equal("ADMIN_OPERATIONS", terminal.Request?.Headers.GetValues("X-Admin-Roles").Single());
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
