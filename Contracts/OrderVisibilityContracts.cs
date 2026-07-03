namespace Marketplace.Web.Contracts;

public sealed record OrderJourneySummary(
    Guid Id,
    Guid? OrderId,
    Guid? CheckoutId,
    Guid? BuyerId,
    Guid? SellerId,
    string CurrentStatus,
    string CurrentStep,
    string? LastEventType,
    DateTimeOffset? LastEventAt,
    string CorrelationId,
    string? RootTraceId,
    bool HasError,
    string? ErrorReason,
    DateTimeOffset UpdatedAt,
    double SecondsSinceLastEvent);

public sealed record OrderJourneyEventDetail(
    Guid Id,
    Guid? OrderId,
    Guid? CheckoutId,
    Guid EventId,
    string EventType,
    string Topic,
    int? Partition,
    long? OffsetValue,
    string? ServiceName,
    string? StatusBefore,
    string? StatusAfter,
    string CorrelationId,
    string? TraceId,
    string? SpanId,
    DateTimeOffset OccurredAt,
    DateTimeOffset ConsumedAt,
    string PayloadJson);

public sealed record OrderJourneyPagedResult(
    IReadOnlyList<OrderJourneySummary> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record OrderJourneyListFilter(
    string? Status,
    bool? HasError,
    bool? StuckOnly,
    Guid? OrderId,
    Guid? CheckoutId,
    string? CorrelationId,
    Guid? BuyerId,
    Guid? SellerId);
