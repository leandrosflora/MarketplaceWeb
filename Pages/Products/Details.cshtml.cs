using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Products;

public sealed class DetailsModel : PageModel
{
    private readonly IMarketplaceBffClient _bffClient;

    public DetailsModel(IMarketplaceBffClient bffClient)
    {
        _bffClient = bffClient;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ZipCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Quantity { get; set; } = 1;

    public ProductPageResponse ProductPage { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var loaded = await LoadProductPageAsync(cancellationToken);

        return loaded ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostBuyAsync(CancellationToken cancellationToken)
    {
        var loaded = await LoadProductPageAsync(cancellationToken);

        if (!loaded)
        {
            return NotFound();
        }

        var normalizedZipCode = NormalizeZipCode(ZipCode);

        if (normalizedZipCode is null)
        {
            ModelState.AddModelError(string.Empty, "Informe um CEP válido para continuar a compra.");
            return Page();
        }

        if (ProductPage.Shipping is not { Available: true, PromiseId: not null })
        {
            ModelState.AddModelError(string.Empty, "Calcule uma entrega disponível antes de continuar a compra.");
            return Page();
        }

        var checkout = await _bffClient.CreateCheckoutAsync(
            new CreateCheckoutRequest(
                [
                    new CreateCheckoutItemRequest(
                        ProductPage.Product.SkuId,
                        ProductPage.Product.SellerId,
                        Quantity)
                ],
                normalizedZipCode,
                ProductPage.Shipping.PromiseId),
            Guid.NewGuid().ToString("N"),
            cancellationToken);

        return RedirectToPage("/Checkout/Review", new { checkoutId = checkout.CheckoutId });
    }

    private async Task<bool> LoadProductPageAsync(CancellationToken cancellationToken)
    {
        Quantity = Math.Clamp(Quantity, 1, 99);

        var response = await _bffClient.GetProductPageAsync(
            Id,
            Quantity,
            NormalizeZipCode(ZipCode),
            cancellationToken);

        if (response is null)
        {
            return false;
        }

        ProductPage = response;

        return true;
    }

    private static string? NormalizeZipCode(string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
        {
            return null;
        }

        var digits = new string(zipCode.Where(char.IsDigit).ToArray());

        return digits.Length == 8 ? digits : null;
    }
}
