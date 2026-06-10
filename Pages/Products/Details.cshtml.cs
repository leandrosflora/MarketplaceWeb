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
        Quantity = Math.Clamp(Quantity, 1, 99);

        var response = await _bffClient.GetProductPageAsync(
            Id,
            Quantity,
            NormalizeZipCode(ZipCode),
            cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        ProductPage = response;

        return Page();
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
