using System.Text.Json.Serialization;

namespace Marketplace.Web.Contracts;

public sealed record ProductResponse(
    Guid SkuId,
    Guid SellerId,
    string Title,
    string Category,
    decimal Price,
    string Status,
    bool AvailableForSale);

public sealed record ProductPageResponse(
    ProductSummary Product,
    ShippingSummary? Shipping,
    IReadOnlyList<string> Warnings);

public sealed record ProductSummary(
    Guid SkuId,
    Guid SellerId,
    string Title,
    string Category,
    decimal Price,
    bool AvailableForSale);

public sealed record ShippingSummary(
    bool Available,
    string? PromiseId,
    string? Mode,
    DateOnly? EstimatedDeliveryDate,
    decimal? Cost,
    string? UnavailableReason);

public sealed class ProductSearchResponse
{
    [JsonPropertyName("products")]
    public IReadOnlyList<ProductSearchItem> Products { get; init; } = [];

    [JsonPropertyName("items")]
    public IReadOnlyList<ProductSearchItem>? Items
    {
        get => Products;
        init => Products = value ?? [];
    }

    [JsonPropertyName("page")]
    public int? Page { get; init; }

    [JsonPropertyName("pageSize")]
    public int? PageSize { get; init; }

    [JsonPropertyName("totalItems")]
    public int? TotalItems { get; init; }

    [JsonPropertyName("totalPages")]
    public int? TotalPages { get; init; }
}

public sealed record ProductSearchItem(
    Guid SkuId,
    string? SellerId,
    string Title,
    string Category,
    decimal Price,
    string Status,
    decimal? Score = null)
{
    public bool AvailableForSale => string.Equals(
        Status,
        "Active",
        StringComparison.OrdinalIgnoreCase);
}

public sealed record ShippingPromiseRequest(
    Guid SkuId,
    Guid SellerId,
    int Quantity,
    string ZipCode,
    string? Region);

public sealed record ShippingPromiseResponse(
    bool Available,
    string? PromiseId,
    Guid? PricingQuoteId,
    string? Mode,
    string? CarrierCode,
    DateOnly? EstimatedDeliveryDate,
    decimal? Price,
    decimal? Cost,
    string Currency,
    string? UnavailableReason);
