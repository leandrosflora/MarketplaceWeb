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

    public async Task<ProductPageResponse?> GetProductPageAsync(
        Guid skuId,
        int quantity,
        string? zipCode,
        CancellationToken cancellationToken)
    {
        var url = $"/api/web/v1/products/{skuId}/page?quantity={quantity}";

        if (!string.IsNullOrWhiteSpace(zipCode))
        {
            url += $"&zipCode={Uri.EscapeDataString(zipCode)}";
        }

        return await GetOrNullAsync<ProductPageResponse>(url, cancellationToken);
    }

    public Task<CheckoutPageResponse?> GetCheckoutAsync(
        Guid checkoutId,
        CancellationToken cancellationToken)
    {
        return GetOrNullAsync<CheckoutPageResponse>(
            $"/api/web/v1/checkouts/{checkoutId}",
            cancellationToken);
    }

    public async Task<ConfirmCheckoutResponse> ConfirmCheckoutAsync(
        ConfirmCheckoutInput input,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/web/v1/checkouts/{input.CheckoutId}/confirm")
        {
            Content = JsonContent.Create(
                new ConfirmCheckoutRequest(
                    input.ShippingPromiseId,
                    input.PricingQuoteId,
                    input.PaymentMethodToken))
        };

        request.Headers.Add("Idempotency-Key", input.IdempotencyKey);

        return await SendAsync<ConfirmCheckoutResponse>(request, cancellationToken);
    }

    public Task<OrderPageResponse?> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return GetOrNullAsync<OrderPageResponse>(
            $"/api/web/v1/orders/{orderId}",
            cancellationToken);
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
