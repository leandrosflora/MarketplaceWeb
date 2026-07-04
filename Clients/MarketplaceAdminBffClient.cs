using System.Net;
using System.Net.Http.Json;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Clients;

public sealed class MarketplaceAdminBffClient : IMarketplaceAdminBffClient
{
    private readonly HttpClient _httpClient;

    public MarketplaceAdminBffClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AdminProductResponse> CreateProductAsync(CreateAdminProductRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/products")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminProductResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminProductResponse?> GetProductAsync(Guid skuId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/products/{skuId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminProductResponse>(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminProductResponse>> ListProductsAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("/admin/products", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminProductResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminProductResponse> UpdateProductLogisticsAsync(Guid skuId, UpdateAdminProductLogisticsRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/admin/products/{skuId}/logistics")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminProductResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminProductResponse> ChangeProductStatusAsync(Guid skuId, ChangeAdminProductStatusRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/admin/products/{skuId}/status")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminProductResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminInventoryItemResponse>> SearchInventoryAsync(
        Guid? sellerId, Guid? skuId, Guid? fulfillmentCenterId, CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (sellerId is { } s) query.Add($"sellerId={s}");
        if (skuId is { } sk) query.Add($"skuId={sk}");
        if (fulfillmentCenterId is { } fc) query.Add($"fulfillmentCenterId={fc}");

        var queryString = query.Count == 0 ? string.Empty : $"?{string.Join('&', query)}";

        using var response = await _httpClient.GetAsync($"/admin/inventory{queryString}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminInventoryItemResponse>>(cancellationToken) ?? [];
    }

    public async Task AdjustStockAsync(AdminStockAdjustmentRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/inventory/adjustments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("Idempotency-Key", idempotencyKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminFulfillmentCenterResponse>> ListFulfillmentCentersAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("/admin/fulfillment-centers", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminFulfillmentCenterResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminFulfillmentCenterResponse?> GetFulfillmentCenterAsync(Guid fulfillmentCenterId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/fulfillment-centers/{fulfillmentCenterId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminFulfillmentCenterResponse>(cancellationToken);
    }

    public async Task<AdminFulfillmentCenterResponse> CreateFulfillmentCenterAsync(CreateAdminFulfillmentCenterRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/fulfillment-centers")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminFulfillmentCenterResponse>(httpRequest, cancellationToken);
    }

    public async Task ChangeFulfillmentCenterStatusAsync(Guid fulfillmentCenterId, ChangeAdminFulfillmentCenterStatusRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/admin/fulfillment-centers/{fulfillmentCenterId}/status")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminCapacitySlotResponse>> GetCapacitySlotsAsync(Guid fulfillmentCenterId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/fulfillment-centers/{fulfillmentCenterId}/capacity", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminCapacitySlotResponse>>(cancellationToken) ?? [];
    }

    public async Task ConfigureCapacityAsync(Guid fulfillmentCenterId, AdminConfigureCapacityRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/admin/fulfillment-centers/{fulfillmentCenterId}/capacity")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminLogisticsNodeResponse>> ListNodesAsync(string? region, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(region) ? string.Empty : $"?region={Uri.EscapeDataString(region)}";
        using var response = await _httpClient.GetAsync($"/admin/network/nodes{query}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminLogisticsNodeResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminLogisticsNodeResponse> CreateNodeAsync(CreateAdminNodeRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/network/nodes")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminLogisticsNodeResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminLogisticsLaneResponse>> ListLanesAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("/admin/network/lanes", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminLogisticsLaneResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminLogisticsLaneResponse> CreateLaneAsync(CreateAdminLaneRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/network/lanes")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminLogisticsLaneResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminLogisticsLaneResponse> UpdateLaneAsync(Guid laneId, UpdateAdminLaneRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/admin/network/lanes/{laneId}")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminLogisticsLaneResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminLogisticsLaneResponse> ChangeLaneStatusAsync(Guid laneId, ChangeAdminLaneStatusRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/admin/network/lanes/{laneId}/status")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminLogisticsLaneResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminCarrierResponse>> ListCarriersAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("/admin/carriers", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminCarrierResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminCarrierResponse> CreateCarrierAsync(CreateAdminCarrierRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/carriers")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminCarrierResponse>(httpRequest, cancellationToken);
    }

    public async Task ChangeCarrierStatusAsync(string carrierCode, ChangeAdminCarrierStatusRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/admin/carriers/{carrierCode}/status")
        {
            Content = JsonContent.Create(request)
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminServiceLevelResponse>> ListServiceLevelsAsync(string carrierCode, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/carriers/{carrierCode}/service-levels", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminServiceLevelResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminServiceLevelResponse> CreateServiceLevelAsync(string carrierCode, CreateAdminServiceLevelRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/admin/carriers/{carrierCode}/service-levels")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminServiceLevelResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminCarrierLaneResponse>> ListCarrierLanesAsync(string carrierCode, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/carriers/{carrierCode}/lanes", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminCarrierLaneResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminCarrierLaneResponse> CreateCarrierLaneAsync(string carrierCode, CreateAdminCarrierLaneRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/admin/carriers/{carrierCode}/lanes")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminCarrierLaneResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminRateCardResponse>> ListRateCardsAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("/admin/rate-cards", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminRateCardResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminRateCardResponse> CreateRateCardAsync(AdminRateCardRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/rate-cards")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminRateCardResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminRateCardResponse> ActivateRateCardAsync(Guid rateCardId, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/admin/rate-cards/{rateCardId}/activate");
        return await SendAsync<AdminRateCardResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminRateCardResponse> RetireRateCardAsync(Guid rateCardId, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/admin/rate-cards/{rateCardId}/retire");
        return await SendAsync<AdminRateCardResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminPromotionRuleResponse>> ListSubsidyRulesAsync(CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync("/admin/subsidy-rules", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminPromotionRuleResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminPromotionRuleResponse> CreateSubsidyRuleAsync(CreateAdminPromotionRuleRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/subsidy-rules")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminPromotionRuleResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminPromotionRuleResponse> ChangeSubsidyRuleActiveAsync(Guid promotionId, bool isActive, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/admin/subsidy-rules/{promotionId}/active")
        {
            Content = JsonContent.Create(new { IsActive = isActive })
        };

        return await SendAsync<AdminPromotionRuleResponse>(httpRequest, cancellationToken);
    }

    public async Task<AdminShippingPromiseSimulationResponse> SimulateShippingPromiseAsync(SimulateAdminShippingPromiseRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/simulators/shipping-promise")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminShippingPromiseSimulationResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminOrderListItemResponse>> ListOrdersAsync(
        Guid? buyerId, Guid? sellerId, string? status, CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (buyerId is { } b) query.Add($"buyerId={b}");
        if (sellerId is { } s) query.Add($"sellerId={s}");
        if (!string.IsNullOrWhiteSpace(status)) query.Add($"status={Uri.EscapeDataString(status)}");

        var queryString = query.Count == 0 ? string.Empty : $"?{string.Join('&', query)}";

        using var response = await _httpClient.GetAsync($"/admin/orders/{queryString}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminOrderListItemResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminOrderResponse?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/orders/{orderId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminOrderResponse>(cancellationToken);
    }

    public async Task CancelOrderAsync(Guid orderId, CancelAdminOrderRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/admin/orders/{orderId}/cancel")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("Idempotency-Key", idempotencyKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminShipmentListItemResponse>> ListShipmentsAsync(Guid? orderId, CancellationToken cancellationToken)
    {
        var queryString = orderId is { } o ? $"?orderId={o}" : string.Empty;

        using var response = await _httpClient.GetAsync($"/admin/shipments/{queryString}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminShipmentListItemResponse>>(cancellationToken) ?? [];
    }

    public async Task<AdminShipmentResponse?> GetShipmentAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/shipments/{shipmentId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminShipmentResponse>(cancellationToken);
    }

    public async Task<AdminShipmentLabelResponse?> GetShipmentLabelAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/shipments/{shipmentId}/label", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminShipmentLabelResponse>(cancellationToken);
    }

    public async Task CancelShipmentAsync(Guid shipmentId, string idempotencyKey, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/admin/shipments/{shipmentId}/cancel");
        httpRequest.Headers.Add("Idempotency-Key", idempotencyKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<AdminShipmentTrackingResponse?> GetShipmentTrackingAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"/admin/tracking/shipments/{shipmentId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<AdminShipmentTrackingResponse>(cancellationToken);
    }

    public async Task<AdminTrackingEventAcceptedResponse> CreateTrackingEventAsync(CreateAdminTrackingEventRequest request, CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/admin/tracking/events")
        {
            Content = JsonContent.Create(request)
        };

        return await SendAsync<AdminTrackingEventAcceptedResponse>(httpRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminAuditLogEntryResponse>> ListAuditLogAsync(
        string? entityType, string? action, string? adminUserId, int? limit, CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(entityType)) query.Add($"entityType={Uri.EscapeDataString(entityType)}");
        if (!string.IsNullOrWhiteSpace(action)) query.Add($"action={Uri.EscapeDataString(action)}");
        if (!string.IsNullOrWhiteSpace(adminUserId)) query.Add($"adminUserId={Uri.EscapeDataString(adminUserId)}");
        if (limit is { } l) query.Add($"limit={l}");

        var queryString = query.Count == 0 ? string.Empty : $"?{string.Join('&', query)}";

        using var response = await _httpClient.GetAsync($"/admin/audit-log/{queryString}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<AdminAuditLogEntryResponse>>(cancellationToken) ?? [];
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            ?? throw new BffApiException(HttpStatusCode.BadGateway, "The Admin BFF returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetails? problem = null;

        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        }
        catch
        {
            // Do not expose technical response bodies directly to the UI.
        }

        throw new BffApiException(
            response.StatusCode,
            problem?.Detail ?? problem?.Title ?? "The operation could not be completed.");
    }
}
