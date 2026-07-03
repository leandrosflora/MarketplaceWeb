using System.Net.Http.Json;
using System.Text.Json;
using Marketplace.Web.Contracts;

namespace Marketplace.Web.Clients;

public sealed class OrderVisibilityClient : IOrderVisibilityClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public OrderVisibilityClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderJourneyPagedResult> ListJourneysAsync(OrderJourneyListFilter filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (filter.StuckOnly == true)
        {
            var stuck = await ListStuckJourneysAsync(60, cancellationToken);
            return new OrderJourneyPagedResult(stuck, 1, stuck.Count, stuck.Count);
        }

        var parameters = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(filter.Status)) parameters.Add($"status={Uri.EscapeDataString(filter.Status)}");
        if (filter.HasError is { } hasError) parameters.Add($"hasError={hasError}");
        if (filter.OrderId is { } orderId) parameters.Add($"orderId={orderId}");
        if (filter.CheckoutId is { } checkoutId) parameters.Add($"checkoutId={checkoutId}");
        if (filter.CorrelationId is { } correlationId) parameters.Add($"correlationId={Uri.EscapeDataString(correlationId)}");
        if (filter.BuyerId is { } buyerId) parameters.Add($"buyerId={buyerId}");
        if (filter.SellerId is { } sellerId) parameters.Add($"sellerId={sellerId}");

        var response = await _httpClient.GetFromJsonAsync<OrderJourneyPagedResult>(
            $"/order-journeys?{string.Join('&', parameters)}", JsonOptions, cancellationToken);
        return response ?? new OrderJourneyPagedResult([], page, pageSize, 0);
    }

    public async Task<IReadOnlyList<OrderJourneySummary>> ListStuckJourneysAsync(int olderThanSeconds, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<List<OrderJourneySummary>>(
            $"/order-journeys/stuck?olderThanSeconds={olderThanSeconds}", JsonOptions, cancellationToken);
        return response ?? [];
    }

    public async Task<OrderJourneySummary?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/order-journeys/{orderId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<OrderJourneySummary>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderJourneyEventDetail>?> GetEventsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/order-journeys/{orderId}/events", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<List<OrderJourneyEventDetail>>(JsonOptions, cancellationToken);
    }
}
