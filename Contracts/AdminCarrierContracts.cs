namespace Marketplace.Web.Contracts;

public sealed record CreateAdminCarrierRequest(string Code, string Name, bool RequiresRealTimeValidation);

public sealed record ChangeAdminCarrierStatusRequest(string Status, string? Reason);

public sealed record AdminCarrierResponse(string Code, string Name, string Status, bool RequiresRealTimeValidation);

public sealed record CreateAdminServiceLevelRequest(
    string Code,
    string Name,
    string Mode,
    decimal MaximumWeightKg,
    decimal MaximumCubicWeightKg,
    bool SupportsFragileItems,
    bool SupportsRestrictedItems,
    int Priority);

public sealed record AdminServiceLevelResponse(
    Guid Id,
    string Code,
    string Name,
    string Mode,
    decimal MaximumWeightKg,
    decimal MaximumCubicWeightKg,
    bool SupportsFragileItems,
    bool SupportsRestrictedItems,
    int Priority,
    bool IsActive);

public sealed record CreateAdminCarrierLaneRequest(
    string ServiceLevelCode,
    Guid OriginNodeId,
    Guid DestinationNodeId,
    string TimeZoneId,
    string CutoffTime,
    HashSet<string> OperatingDays);

public sealed record AdminCarrierLaneResponse(
    Guid Id,
    string ServiceLevelCode,
    Guid OriginNodeId,
    Guid DestinationNodeId,
    string TimeZoneId,
    string CutoffTime,
    bool OperatesOnMonday,
    bool OperatesOnTuesday,
    bool OperatesOnWednesday,
    bool OperatesOnThursday,
    bool OperatesOnFriday,
    bool OperatesOnSaturday,
    bool OperatesOnSunday,
    bool IsActive);
