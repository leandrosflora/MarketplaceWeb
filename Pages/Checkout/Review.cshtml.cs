using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Checkout;

public sealed class ReviewModel : PageModel
{
    private readonly IMarketplaceBffClient _bffClient;

    public ReviewModel(IMarketplaceBffClient bffClient)
    {
        _bffClient = bffClient;
    }

    [BindProperty(SupportsGet = true)]
    public Guid CheckoutId { get; set; }

    [BindProperty]
    public ConfirmCheckoutInput Input { get; set; } = new();

    public CheckoutPageResponse Checkout { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var loaded = await LoadCheckoutAsync(cancellationToken);

        if (!loaded)
        {
            return NotFound();
        }

        Input = new ConfirmCheckoutInput
        {
            CheckoutId = Checkout.CheckoutId,
            ShippingPromiseId = Checkout.SelectedShipping.PromiseId,
            PricingQuoteId = Checkout.SelectedShipping.PricingQuoteId,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        };

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCheckoutAsync(cancellationToken);
            return Page();
        }

        try
        {
            var response = await _bffClient.ConfirmCheckoutAsync(Input, cancellationToken);

            return RedirectToPage("/Orders/Details", new { id = response.OrderId });
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadCheckoutAsync(cancellationToken);

            return Page();
        }
    }

    private async Task<bool> LoadCheckoutAsync(CancellationToken cancellationToken)
    {
        var checkoutId = CheckoutId != Guid.Empty ? CheckoutId : Input.CheckoutId;

        var response = await _bffClient.GetCheckoutAsync(checkoutId, cancellationToken);

        if (response is null)
        {
            return false;
        }

        Checkout = response;

        return true;
    }
}
