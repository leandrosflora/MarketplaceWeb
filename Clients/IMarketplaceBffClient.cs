using Marketplace.Web.Contracts;

namespace Marketplace.Web.Clients;

public interface IMarketplaceBffClient
{
    Task<ProductResponse?> GetProductAsync(
        Guid skuId,
        CancellationToken cancellationToken);

    Task<ProductPageResponse?> GetProductPageAsync(
        Guid skuId,
        int quantity,
        string? zipCode,
        CancellationToken cancellationToken);

    Task<ProductSearchResponse> SearchProductsAsync(
        string query,
        int? page,
        int? pageSize,
        string? zipCode,
        string? region,
        CancellationToken cancellationToken);

    Task<ShippingPromiseResponse> CalculateShippingPromiseAsync(
        ShippingPromiseRequest input,
        CancellationToken cancellationToken);

    Task<CheckoutPageResponse> CreateCheckoutAsync(
        CreateCheckoutRequest input,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<CheckoutPageResponse?> GetCheckoutAsync(
        Guid checkoutId,
        CancellationToken cancellationToken);

    Task<ConfirmCheckoutResponse> ConfirmCheckoutAsync(
        ConfirmCheckoutInput input,
        CancellationToken cancellationToken);

    Task<OrderPageResponse?> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task CancelOrderAsync(
        Guid orderId,
        string reason,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<TrackingSummary?> GetOrderTrackingAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<ShipmentLabelResponse?> GetShipmentLabelAsync(
        Guid shipmentId,
        CancellationToken cancellationToken);
}
