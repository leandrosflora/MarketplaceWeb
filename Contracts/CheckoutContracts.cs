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
    string Title,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public sealed record ShippingOptionResponse(
    string PromiseId,
    Guid PricingQuoteId,
    string Mode,
    DateOnly EstimatedDeliveryDate,
    decimal Price);

public sealed class ConfirmCheckoutInput
{
    [Required]
    public Guid CheckoutId { get; set; }

    [Required]
    public string ShippingPromiseId { get; set; } = string.Empty;

    [Required]
    public Guid PricingQuoteId { get; set; }

    [Required]
    public string PaymentMethodToken { get; set; } = string.Empty;

    [Required]
    public string IdempotencyKey { get; set; } = string.Empty;
}

public sealed record ConfirmCheckoutRequest(
    string ShippingPromiseId,
    Guid PricingQuoteId,
    string PaymentMethodToken);

public sealed record ConfirmCheckoutResponse(
    Guid CheckoutId,
    Guid OrderId,
    string Status);
