namespace Marketplace.Web.Contracts;

public sealed record CreateAdminNodeRequest(
    string Code,
    string Name,
    string Region,
    string TimeZoneId,
    string Type,
    int HandlingMinutes);

public sealed record AdminLogisticsNodeResponse(
    Guid Id,
    string Code,
    string Name,
    string Region,
    string TimeZoneId,
    string Type,
    int HandlingMinutes,
    bool IsActive);

public sealed record AdminLaneScheduleDto(string DayOfWeek, TimeOnly DepartureTime, bool IsActive = true);

public sealed record CreateAdminLaneRequest(
    Guid OriginNodeId,
    Guid DestinationNodeId,
    string CarrierCode,
    string ServiceLevelCode,
    string Mode,
    int TransitMinutes,
    decimal MaximumWeightKg,
    decimal MaximumCubicWeightKg,
    bool SupportsFragileItems,
    bool SupportsRestrictedItems,
    string Region,
    IReadOnlyList<AdminLaneScheduleDto> Schedules);

public sealed record UpdateAdminLaneRequest(
    int TransitMinutes,
    decimal MaximumWeightKg,
    decimal MaximumCubicWeightKg,
    bool SupportsFragileItems,
    bool SupportsRestrictedItems,
    string Region,
    IReadOnlyList<AdminLaneScheduleDto> Schedules);

public sealed record ChangeAdminLaneStatusRequest(string Status, string Region);

public sealed record AdminLogisticsLaneResponse(
    Guid Id,
    Guid OriginNodeId,
    Guid DestinationNodeId,
    string CarrierCode,
    string ServiceLevelCode,
    string Mode,
    int TransitMinutes,
    decimal MaximumWeightKg,
    decimal MaximumCubicWeightKg,
    bool SupportsFragileItems,
    bool SupportsRestrictedItems,
    string Status,
    long Version,
    IReadOnlyList<AdminLaneScheduleDto> Schedules);
