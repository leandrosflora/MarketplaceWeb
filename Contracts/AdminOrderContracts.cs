namespace Marketplace.Web.Contracts;

public sealed record AdminOrderListItemResponse(
    Guid Id,
    Guid BuyerId,
    Guid SellerId,
    string Status,
    decimal ItemsTotal,
    decimal ShippingPrice,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAt,
    Guid? ShipmentId);

public sealed record AdminOrderItemResponse(
    Guid SkuId,
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public sealed record AdminOrderResponse(
    Guid Id,
    Guid CheckoutId,
    Guid BuyerId,
    Guid SellerId,
    string Status,
    string Currency,
    decimal ItemsTotal,
    decimal ShippingPrice,
    decimal TotalAmount,
    string ShippingPromiseId,
    Guid PricingQuoteId,
    Guid? InventoryReservationId,
    Guid? CapacityReservationId,
    Guid? PaymentAuthorizationId,
    Guid? ShipmentId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CancelledAt,
    IReadOnlyList<AdminOrderItemResponse> Items);

public sealed record CancelAdminOrderRequest(string Reason);
