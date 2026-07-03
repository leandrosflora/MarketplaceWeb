namespace Marketplace.Web.Contracts;

public sealed record AdminAuditLogEntryResponse(
    Guid Id,
    string AdminUserId,
    string Action,
    string EntityType,
    string EntityId,
    string? BeforeJson,
    string? AfterJson,
    string CorrelationId,
    DateTimeOffset CreatedAt);
