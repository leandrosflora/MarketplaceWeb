using Marketplace.Web.Contracts;

namespace Marketplace.Web.Clients;

public interface IMarketplaceBffClient
{
    Task<ProductPageResponse?> GetProductPageAsync(
        Guid skuId,
        int quantity,
        string? zipCode,
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
}
