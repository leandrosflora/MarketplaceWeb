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
