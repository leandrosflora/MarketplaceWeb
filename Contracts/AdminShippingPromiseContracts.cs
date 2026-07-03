namespace Marketplace.Web.Contracts;

public sealed record SimulateAdminAddressRequest(string ZipCode, string City, string State, string Country);

public sealed record SimulateAdminShippingPromiseItemRequest(Guid SkuId, int Quantity, decimal UnitPrice);

public sealed record SimulateAdminShippingPromiseRequest(
    Guid? CheckoutId,
    Guid BuyerId,
    Guid SellerId,
    SimulateAdminAddressRequest Destination,
    IReadOnlyList<SimulateAdminShippingPromiseItemRequest> Items);

public sealed record AdminShippingPromiseSimulationResponse(
    bool Available,
    string? PromiseId,
    string? Mode,
    string? Carrier,
    DateOnly? EstimatedDeliveryDate,
    decimal? Cost,
    string Source,
    string? UnavailableReason);
