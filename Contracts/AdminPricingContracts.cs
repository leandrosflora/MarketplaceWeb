namespace Marketplace.Web.Contracts;

public sealed record AdminRateBandRequest(
    Guid OriginNodeId,
    string DestinationZone,
    decimal MinimumWeightKg,
    decimal MaximumWeightKg,
    decimal BasePrice,
    decimal IncludedWeightKg,
    decimal WeightIncrementKg,
    decimal PricePerWeightIncrement,
    decimal FuelSurchargePercentage,
    decimal RemoteAreaFee,
    decimal FragileFee,
    decimal OversizeThresholdKg,
    decimal OversizeFee,
    decimal MinimumLogisticsCost);

public sealed record AdminRateCardRequest(
    string Code,
    string CarrierCode,
    string ServiceLevelCode,
    string Currency,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset EffectiveUntil,
    IReadOnlyList<AdminRateBandRequest> Bands);

public sealed record AdminRateBandResponse(
    Guid Id,
    Guid OriginNodeId,
    string DestinationZone,
    decimal MinimumWeightKg,
    decimal MaximumWeightKg,
    decimal BasePrice,
    decimal IncludedWeightKg,
    decimal WeightIncrementKg,
    decimal PricePerWeightIncrement,
    decimal FuelSurchargePercentage,
    decimal RemoteAreaFee,
    decimal FragileFee,
    decimal OversizeThresholdKg,
    decimal OversizeFee,
    decimal MinimumLogisticsCost);

public sealed record AdminRateCardResponse(
    Guid Id,
    string Code,
    string CarrierCode,
    string ServiceLevelCode,
    string Currency,
    long Version,
    string Status,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset EffectiveUntil,
    IReadOnlyList<AdminRateBandResponse> Bands);

public sealed record CreateAdminPromotionRuleRequest(
    string Code,
    Guid? SellerId,
    int Priority,
    decimal MinimumCartTotal,
    decimal CustomerDiscountPercentage,
    decimal PlatformSubsidyPercentage,
    decimal SellerSubsidyPercentage,
    decimal MaximumBenefit,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);

public sealed record AdminPromotionRuleResponse(
    Guid Id,
    string Code,
    Guid? SellerId,
    int Priority,
    decimal MinimumCartTotal,
    decimal CustomerDiscountPercentage,
    decimal PlatformSubsidyPercentage,
    decimal SellerSubsidyPercentage,
    decimal MaximumBenefit,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool IsActive);
