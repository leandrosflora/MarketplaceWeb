using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MarketplaceWeb.Tests.Auth;

public sealed class AuthenticationFlowTests
{
    [Fact]
    public async Task Login_WithValidCredentials_RedirectsToLocalReturnUrl()
    {
        await using var factory = new MarketplaceWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await PostLoginAsync(client, "admin", "admin123", "/Admin");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Admin", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShowsValidationError()
    {
        await using var factory = new MarketplaceWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await PostLoginAsync(client, "admin", "wrong", "/Admin");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Usuario ou senha invalidos.", body);
        Assert.DoesNotContain("MarketplaceWeb.Auth", response.Headers.GetValuesOrEmpty("Set-Cookie"));
    }

    [Fact]
    public async Task Login_WithUnsafeReturnUrl_RedirectsHome()
    {
        await using var factory = new MarketplaceWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await PostLoginAsync(client, "admin", "admin123", "https://example.com/admin");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Logout_ClearsAuthenticationCookie()
    {
        await using var factory = new MarketplaceWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await PostLoginAsync(client, "admin", "admin123", "/Admin");
        var adminPage = await client.GetAsync("/Admin");
        var token = await ReadAntiforgeryTokenAsync(adminPage);

        var response = await client.PostAsync(
            "/Account/Logout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(
            response.Headers.GetValuesOrEmpty("Set-Cookie"),
            value => value.Contains("MarketplaceWeb.Auth=", StringComparison.Ordinal)
                && value.Contains("expires=", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AnonymousOrders_RedirectsToLoginWithReturnUrl()
    {
        await using var factory = new MarketplaceWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Orders");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login?ReturnUrl=%2FOrders", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task AuthenticatedCustomer_CanAccessOrdersButCannotAccessAdmin()
    {
        await using var factory = new MarketplaceWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await PostLoginAsync(client, "comprador", "comprador123", "/Orders");

        var orders = await client.GetAsync("/Orders");
        var admin = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.OK, orders.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, admin.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", admin.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task AuthenticatedAdmin_CanAccessBackoffice()
    {
        await using var factory = new MarketplaceWebFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await PostLoginAsync(client, "admin", "admin123", "/Admin");

        var response = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(
        HttpClient client,
        string username,
        string password,
        string returnUrl)
    {
        var loginPage = await client.GetAsync($"/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
        var token = await ReadAntiforgeryTokenAsync(loginPage);

        return await client.PostAsync(
            $"/Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Username"] = username,
                ["Input.Password"] = password,
                ["ReturnUrl"] = returnUrl,
                ["__RequestVerificationToken"] = token
            }));
    }

    private static async Task<string> ReadAntiforgeryTokenAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            body,
            "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");

        Assert.True(match.Success, "Expected an antiforgery token in the rendered form.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}

internal sealed class MarketplaceWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Bff:BaseUrl"] = "https://bff.local",
                ["AdminBff:BaseUrl"] = "https://admin-bff.local",
                ["OrderVisibility:BaseUrl"] = "https://visibility.local",
                ["MarketplaceAuth:Users:0:Username"] = "comprador",
                ["MarketplaceAuth:Users:0:Password"] = "comprador123",
                ["MarketplaceAuth:Users:0:DisplayName"] = "Comprador Demo",
                ["MarketplaceAuth:Users:0:UserId"] = "11111111-1111-1111-1111-111111111111",
                ["MarketplaceAuth:Users:0:BuyerId"] = "11111111-1111-1111-1111-111111111111",
                ["MarketplaceAuth:Users:1:Username"] = "admin",
                ["MarketplaceAuth:Users:1:Password"] = "admin123",
                ["MarketplaceAuth:Users:1:DisplayName"] = "Admin Demo",
                ["MarketplaceAuth:Users:1:UserId"] = "dev-operator@local",
                ["MarketplaceAuth:Users:1:Roles:0"] = "ADMIN_OPERATIONS"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMarketplaceBffClient>();
            services.AddSingleton<IMarketplaceBffClient, FakeMarketplaceBffClient>();
        });
    }
}

internal sealed class FakeMarketplaceBffClient : IMarketplaceBffClient
{
    public Task<ProductResponse?> GetProductAsync(Guid skuId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<ProductPageResponse?> GetProductPageAsync(
        Guid skuId,
        int quantity,
        string? zipCode,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<ProductSearchResponse> SearchProductsAsync(
        string query,
        int? page,
        int? pageSize,
        string? zipCode,
        string? region,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<ShippingPromiseResponse> CalculateShippingPromiseAsync(
        ShippingPromiseRequest input,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<CheckoutPageResponse> CreateCheckoutAsync(
        CreateCheckoutRequest input,
        string idempotencyKey,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<CheckoutPageResponse?> GetCheckoutAsync(Guid checkoutId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<CheckoutPageResponse> ConfirmCheckoutAsync(
        ConfirmCheckoutInput input,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<OrderPageResponse?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        Task.FromResult<OrderPageResponse?>(new OrderPageResponse(
            new OrderSummary(orderId, "Confirmed", 10m, 2m, 12m, "BRL", DateTimeOffset.UtcNow),
            null,
            null,
            []));

    public Task<IReadOnlyList<OrderListItemResponse>> ListOrdersAsync(Guid buyerId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<OrderListItemResponse>>(
        [
            new OrderListItemResponse(Guid.NewGuid(), "Confirmed", 10m, 2m, 12m, "BRL", DateTimeOffset.UtcNow, null)
        ]);

    public Task CancelOrderAsync(
        Guid orderId,
        string reason,
        string idempotencyKey,
        CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<TrackingSummary?> GetOrderTrackingAsync(Guid orderId, CancellationToken cancellationToken) =>
        Task.FromResult<TrackingSummary?>(null);

    public Task<ShipmentLabelResponse?> GetShipmentLabelAsync(Guid shipmentId, CancellationToken cancellationToken) =>
        Task.FromResult<ShipmentLabelResponse?>(null);

    public Task<CartResponse> GetCartAsync(string cartOwnerId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<CartResponse> AddCartItemAsync(string cartOwnerId, AddCartItemRequest request, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<CartResponse> UpdateCartItemQuantityAsync(string cartOwnerId, Guid skuId, int quantity, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<CartResponse> RemoveCartItemAsync(string cartOwnerId, Guid skuId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task MergeCartsAsync(string anonymousCartOwnerId, string buyerCartOwnerId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<CartCheckoutResponse> ProceedToCheckoutAsync(string cartOwnerId, ProceedToCheckoutRequest request, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<PaymentMethodResponse> SubmitPaymentMethodAsync(Guid checkoutId, PaymentMethodRequest request, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<PaymentMethodResponse?> GetPaymentMethodAsync(Guid checkoutId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();
}

internal static class HeaderExtensions
{
    public static IEnumerable<string> GetValuesOrEmpty(this HttpResponseHeaders headers, string name)
    {
        return headers.TryGetValues(name, out var values) ? values : [];
    }
}
