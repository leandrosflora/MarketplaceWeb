using System.Text.Json.Serialization;

namespace Marketplace.Web.Contracts;

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
