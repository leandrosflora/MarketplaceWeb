using System.Net;
using System.Net.Http.Json;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Clients;

public sealed class MarketplaceBffClient : IMarketplaceBffClient
{
    private readonly HttpClient _httpClient;

    public MarketplaceBffClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<ProductResponse?> GetProductAsync(
        Guid skuId,
        CancellationToken cancellationToken)
    {
        return GetOrNullAsync<ProductResponse>(
            $"/api/web/v1/products/{skuId}",
            cancellationToken);
    }

    public async Task<ProductPageResponse?> GetProductPageAsync(
        Guid skuId,
        int quantity,
        string? zipCode,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>
        {
            ["quantity"] = quantity.ToString(),
            ["zipCode"] = zipCode
        };

        return await GetOrNullAsync<ProductPageResponse>(
            $"/api/web/v1/products/{skuId}/page{BuildQueryString(query)}",
            cancellationToken);
    }

    public async Task<ProductSearchResponse> SearchProductsAsync(
        string query,
        int? page,
        int? pageSize,
        string? zipCode,
        string? region,
        CancellationToken cancellationToken)
    {
        var queryString = BuildQueryString(new Dictionary<string, string?>
        {
            ["query"] = query,
            ["page"] = page?.ToString(),
            ["pageSize"] = pageSize?.ToString(),
            ["zipCode"] = zipCode,
            ["region"] = region
        });

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/web/v1/products/search{queryString}");

        return await SendAsync<ProductSearchResponse>(request, cancellationToken);
    }

    public async Task<ShippingPromiseResponse> CalculateShippingPromiseAsync(
        ShippingPromiseRequest input,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/web/v1/shipping-promises")
        {
            Content = JsonContent.Create(input)
        };

        return await SendAsync<ShippingPromiseResponse>(request, cancellationToken);
    }

    public async Task<CheckoutPageResponse> CreateCheckoutAsync(
        CreateCheckoutRequest input,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/web/v1/checkouts")
        {
            Content = JsonContent.Create(input)
        };

        request.Headers.Add("Idempotency-Key", idempotencyKey);

        return await SendAsync<CheckoutPageResponse>(request, cancellationToken);
    }

    public Task<CheckoutPageResponse?> GetCheckoutAsync(
        Guid checkoutId,
        CancellationToken cancellationToken)
    {
        return GetOrNullAsync<CheckoutPageResponse>(
            $"/api/web/v1/checkouts/{checkoutId}",
            cancellationToken);
    }

    public async Task<CheckoutPageResponse> ConfirmCheckoutAsync(
        ConfirmCheckoutInput input,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/web/v1/checkouts/{input.CheckoutId}/confirm")
        {
            Content = JsonContent.Create(new ConfirmCheckoutRequest(input.PaymentIntentId))
        };

        request.Headers.Add("Idempotency-Key", input.IdempotencyKey);

        return await SendAsync<CheckoutPageResponse>(request, cancellationToken);
    }

    public Task<OrderPageResponse?> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return GetOrNullAsync<OrderPageResponse>(
            $"/api/web/v1/orders/{orderId}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<OrderListItemResponse>> ListOrdersAsync(
        Guid buyerId,
        CancellationToken cancellationToken)
    {
        var orders = await GetOrNullAsync<IReadOnlyList<OrderListItemResponse>>(
            $"/api/web/v1/orders?buyerId={buyerId}",
            cancellationToken);

        return orders ?? [];
    }

    public async Task CancelOrderAsync(
        Guid orderId,
        string reason,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/web/v1/orders/{orderId}/cancel")
        {
            Content = JsonContent.Create(new { reason })
        };

        request.Headers.Add("Idempotency-Key", idempotencyKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public Task<TrackingSummary?> GetOrderTrackingAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return GetOrNullAsync<TrackingSummary>(
            $"/api/web/v1/orders/{orderId}/tracking",
            cancellationToken);
    }

    public Task<ShipmentLabelResponse?> GetShipmentLabelAsync(
        Guid shipmentId,
        CancellationToken cancellationToken)
    {
        return GetOrNullAsync<ShipmentLabelResponse>(
            $"/api/web/v1/shipments/{shipmentId}/label",
            cancellationToken);
    }

    private async Task<T?> GetOrNullAsync<T>(
        string url,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    public Task<CartResponse> GetCartAsync(string cartOwnerId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/web/v1/cart{BuildQueryString(new Dictionary<string, string?> { ["cartOwnerId"] = cartOwnerId })}");

        return SendAsync<CartResponse>(request, cancellationToken);
    }

    public Task<CartResponse> AddCartItemAsync(string cartOwnerId, AddCartItemRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/web/v1/cart/items{BuildQueryString(new Dictionary<string, string?> { ["cartOwnerId"] = cartOwnerId })}")
        {
            Content = JsonContent.Create(request)
        };

        return SendAsync<CartResponse>(httpRequest, cancellationToken);
    }

    public Task<CartResponse> UpdateCartItemQuantityAsync(string cartOwnerId, Guid skuId, int quantity, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Put,
            $"/api/web/v1/cart/items/{skuId}{BuildQueryString(new Dictionary<string, string?> { ["cartOwnerId"] = cartOwnerId })}")
        {
            Content = JsonContent.Create(new UpdateCartItemQuantityRequest(quantity))
        };

        return SendAsync<CartResponse>(httpRequest, cancellationToken);
    }

    public Task<CartResponse> RemoveCartItemAsync(string cartOwnerId, Guid skuId, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/web/v1/cart/items/{skuId}{BuildQueryString(new Dictionary<string, string?> { ["cartOwnerId"] = cartOwnerId })}");

        return SendAsync<CartResponse>(httpRequest, cancellationToken);
    }

    public async Task MergeCartsAsync(string anonymousCartOwnerId, string buyerCartOwnerId, CancellationToken cancellationToken)
    {
        var queryString = BuildQueryString(new Dictionary<string, string?>
        {
            ["anonymousCartOwnerId"] = anonymousCartOwnerId,
            ["buyerCartOwnerId"] = buyerCartOwnerId
        });

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/web/v1/cart/merge{queryString}");
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public Task<CartCheckoutResponse> ProceedToCheckoutAsync(string cartOwnerId, ProceedToCheckoutRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/web/v1/cart/checkout{BuildQueryString(new Dictionary<string, string?> { ["cartOwnerId"] = cartOwnerId })}")
        {
            Content = JsonContent.Create(request)
        };

        return SendAsync<CartCheckoutResponse>(httpRequest, cancellationToken);
    }

    public Task<PaymentMethodResponse> SubmitPaymentMethodAsync(Guid checkoutId, PaymentMethodRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/web/v1/checkouts/{checkoutId}/payment-method")
        {
            Content = JsonContent.Create(request)
        };

        return SendAsync<PaymentMethodResponse>(httpRequest, cancellationToken);
    }

    public Task<PaymentMethodResponse?> GetPaymentMethodAsync(Guid checkoutId, CancellationToken cancellationToken)
    {
        return GetOrNullAsync<PaymentMethodResponse>($"/api/web/v1/checkouts/{checkoutId}/payment-method", cancellationToken);
    }

    private static string BuildQueryString(IReadOnlyDictionary<string, string?> parameters)
    {
        var values = parameters
            .Where(parameter => !string.IsNullOrWhiteSpace(parameter.Value))
            .Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value!)}");

        var queryString = string.Join("&", values);

        return string.IsNullOrWhiteSpace(queryString) ? string.Empty : $"?{queryString}";
    }

    private async Task<T> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            ?? throw new BffApiException(
                HttpStatusCode.BadGateway,
                "The BFF returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetails? problem = null;

        try
        {
            problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        }
        catch
        {
            // Do not expose technical response bodies directly to the UI.
        }

        throw new BffApiException(
            response.StatusCode,
            problem?.Detail
            ?? problem?.Title
            ?? "The operation could not be completed.");
    }
}
