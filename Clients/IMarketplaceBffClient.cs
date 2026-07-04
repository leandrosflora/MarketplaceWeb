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

    Task<CheckoutPageResponse> ConfirmCheckoutAsync(
        ConfirmCheckoutInput input,
        CancellationToken cancellationToken);

    Task<OrderPageResponse?> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderListItemResponse>> ListOrdersAsync(
        Guid buyerId,
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

    Task<CartResponse> GetCartAsync(string cartOwnerId, CancellationToken cancellationToken);

    Task<CartResponse> AddCartItemAsync(string cartOwnerId, AddCartItemRequest request, CancellationToken cancellationToken);

    Task<CartResponse> UpdateCartItemQuantityAsync(string cartOwnerId, Guid skuId, int quantity, CancellationToken cancellationToken);

    Task<CartResponse> RemoveCartItemAsync(string cartOwnerId, Guid skuId, CancellationToken cancellationToken);

    Task MergeCartsAsync(string anonymousCartOwnerId, string buyerCartOwnerId, CancellationToken cancellationToken);

    Task<CartCheckoutResponse> ProceedToCheckoutAsync(string cartOwnerId, ProceedToCheckoutRequest request, CancellationToken cancellationToken);

    Task<PaymentMethodResponse> SubmitPaymentMethodAsync(Guid checkoutId, PaymentMethodRequest request, CancellationToken cancellationToken);

    Task<PaymentMethodResponse?> GetPaymentMethodAsync(Guid checkoutId, CancellationToken cancellationToken);
}
