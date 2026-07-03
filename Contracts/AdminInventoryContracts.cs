namespace Marketplace.Web.Contracts;

public sealed record AdminInventoryItemResponse(
    Guid SellerId,
    Guid SkuId,
    Guid FulfillmentCenterId,
    int OnHandQuantity,
    int ReservedQuantity,
    int AvailableQuantity,
    DateTimeOffset UpdatedAt);

public sealed record AdminStockAdjustmentRequest(
    Guid SellerId,
    Guid SkuId,
    Guid FulfillmentCenterId,
    int QuantityDelta,
    string Reason,
    string? Description = null);
