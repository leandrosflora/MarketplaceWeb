namespace Marketplace.Web.Contracts;

public sealed record CreateAdminFulfillmentCenterRequest(
    string Code,
    string Name,
    string Region,
    string TimeZoneId,
    decimal MaximumWeightKg,
    decimal MaximumCubicWeightKg,
    bool SupportsFragileItems,
    bool SupportsRestrictedItems);

public sealed record AdminFulfillmentCenterResponse(
    Guid Id,
    string Code,
    string Name,
    string Region,
    string TimeZoneId,
    string Status,
    decimal MaximumWeightKg,
    decimal MaximumCubicWeightKg,
    bool SupportsFragileItems,
    bool SupportsRestrictedItems);

public sealed record ChangeAdminFulfillmentCenterStatusRequest(string Status);

public sealed record AdminConfigureCapacityRequest(DateOnly OperationDate, string Mode, int TotalCapacityUnits);

public sealed record AdminCapacitySlotResponse(
    Guid FulfillmentCenterId,
    DateOnly OperationDate,
    string Mode,
    int TotalCapacityUnits,
    int ReservedCapacityUnits,
    int ConsumedCapacityUnits,
    int AvailableCapacityUnits,
    decimal UtilizationPercentage);
