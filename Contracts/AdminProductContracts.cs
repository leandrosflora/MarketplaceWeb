namespace Marketplace.Web.Contracts;

public sealed record AdminProductDimensions(decimal HeightCm, decimal WidthCm, decimal LengthCm);

public sealed record CreateAdminProductRequest(
    Guid SellerId,
    Guid SkuId,
    string Title,
    string Category,
    decimal Price,
    AdminProductDimensions Dimensions,
    decimal WeightKg,
    bool IsFragile,
    bool IsRestricted);

public sealed record UpdateAdminProductLogisticsRequest(
    decimal WeightKg,
    AdminProductDimensions Dimensions,
    bool IsFragile,
    bool IsRestricted);

public sealed record ChangeAdminProductStatusRequest(string Status);

public sealed record AdminProductResponse(
    Guid SkuId,
    Guid SellerId,
    string Title,
    string Category,
    decimal Price,
    string Status,
    decimal WeightKg,
    decimal HeightCm,
    decimal WidthCm,
    decimal LengthCm,
    bool IsFragile,
    bool IsRestricted);
