using System.ComponentModel.DataAnnotations;

namespace Marketplace.Web.Contracts;

public sealed record CheckoutPageResponse(
    Guid CheckoutId,
    decimal ItemsTotal,
    decimal ShippingPrice,
    decimal TotalAmount,
    string Currency,
    ShippingOptionResponse SelectedShipping,
    IReadOnlyList<CheckoutItemResponse> Items);

public sealed record CheckoutItemResponse(
    Guid SkuId,
    int Quantity);

public sealed record ShippingOptionResponse(
    string? PromiseId,
    string? Mode,
    string? Carrier,
    DateOnly? EstimatedDeliveryDate,
    decimal Cost);

public sealed record CreateCheckoutRequest(
    Guid BuyerId,
    Guid SellerId,
    ShippingAddressRequest ShippingAddress,
    IReadOnlyList<CreateCheckoutItemRequest> Items);

public sealed record ShippingAddressRequest(
    string ZipCode,
    string City,
    string State,
    string Country);

public sealed record CreateCheckoutItemRequest(
    Guid SkuId,
    int Quantity,
    decimal UnitPrice);

public sealed class ConfirmCheckoutInput
{
    [Required]
    public Guid CheckoutId { get; set; }

    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;

    [Required]
    public string IdempotencyKey { get; set; } = string.Empty;
}

public sealed record ConfirmCheckoutRequest(string PaymentIntentId);
