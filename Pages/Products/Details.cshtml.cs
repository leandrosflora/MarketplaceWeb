using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Marketplace.Web.Infrastructure.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Products;

public sealed class DetailsModel : PageModel
{
    private readonly IMarketplaceBffClient _bffClient;
    private readonly ICartOwnerIdAccessor _cartOwnerIdAccessor;

    public DetailsModel(IMarketplaceBffClient bffClient, ICartOwnerIdAccessor cartOwnerIdAccessor)
    {
        _bffClient = bffClient;
        _cartOwnerIdAccessor = cartOwnerIdAccessor;
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

    public async Task<IActionResult> OnPostAddToCartAsync(CancellationToken cancellationToken)
    {
        var loaded = await LoadProductPageAsync(cancellationToken);

        if (!loaded)
        {
            return NotFound();
        }

        if (!ProductPage.Product.AvailableForSale)
        {
            ModelState.AddModelError(string.Empty, "Este produto não pode ser comprado no momento.");

            return Page();
        }

        var cartOwnerId = _cartOwnerIdAccessor.GetOrCreateCartOwnerId(HttpContext);

        await _bffClient.AddCartItemAsync(
            cartOwnerId,
            new AddCartItemRequest(
                ProductPage.Product.SkuId,
                ProductPage.Product.SellerId,
                ProductPage.Product.Title,
                ProductPage.Product.Price,
                Quantity),
            cancellationToken);

        return RedirectToPage("/Cart/Index");
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
