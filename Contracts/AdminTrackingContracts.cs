namespace Marketplace.Web.Contracts;

public sealed record AdminTrackingLocationDto(string? FacilityCode, string? City, string? State, string? Country);

public sealed record AdminTrackingEventResponse(
    Guid EventId,
    string Status,
    string? Description,
    string? ExceptionCode,
    AdminTrackingLocationDto? Location,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReceivedAt);

public sealed record AdminShipmentTrackingResponse(
    Guid ShipmentId,
    string TrackingCode,
    string CarrierCode,
    string CurrentStatus,
    AdminTrackingLocationDto? LastLocation,
    DateTimeOffset LastEventOccurredAt,
    DateOnly? EstimatedDeliveryDate,
    DateTimeOffset? DeliveredAt,
    string? CurrentExceptionCode,
    IReadOnlyList<AdminTrackingEventResponse> Events);

public sealed record CreateAdminTrackingEventRequest(
    Guid ShipmentId,
    Guid OrderId,
    Guid BuyerId,
    string TrackingCode,
    string CarrierCode,
    string Status,
    string? Description,
    string? ExceptionCode,
    AdminTrackingLocationDto? Location,
    DateTimeOffset OccurredAt,
    DateOnly? EstimatedDeliveryDate);

public sealed record AdminTrackingEventAcceptedResponse(
    Guid MessageId,
    string CorrelationId,
    Guid ShipmentId,
    string ProviderEventId,
    string Status);
