namespace Marketplace.Web.Contracts;

public sealed record AdminShipmentListItemResponse(
    Guid Id,
    Guid OrderId,
    string Status,
    string? CarrierCode,
    string? ServiceLevelCode,
    string? TrackingCode,
    DateTimeOffset CreatedAt);

public sealed record AdminShipmentPackageItemResponse(Guid SkuId, int Quantity);

public sealed record AdminShipmentPackageResponse(
    int Sequence,
    decimal WeightKg,
    decimal HeightCm,
    decimal WidthCm,
    decimal LengthCm,
    IReadOnlyList<AdminShipmentPackageItemResponse> Items);

public sealed record AdminShipmentResponse(
    Guid Id,
    Guid OrderId,
    string Status,
    string? CarrierCode,
    string? ServiceLevelCode,
    string? TrackingCode,
    DateOnly PromisedDeliveryDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadyAt,
    IReadOnlyList<AdminShipmentPackageResponse> Packages);

public sealed record AdminShipmentLabelResponse(string Url, int ExpiresInSeconds);
