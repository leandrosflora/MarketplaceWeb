using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Products;

public sealed class LogisticsModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public LogisticsModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty(SupportsGet = true)]
    public Guid SkuId { get; set; }

    [BindProperty]
    public LogisticsInput Input { get; set; } = new();

    public AdminProductResponse? Product { get; private set; }

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var loaded = await LoadAsync(cancellationToken);

        if (!loaded)
        {
            return NotFound();
        }

        Input = new LogisticsInput
        {
            WeightKg = Product!.WeightKg,
            HeightCm = Product.HeightCm,
            WidthCm = Product.WidthCm,
            LengthCm = Product.LengthCm,
            IsFragile = Product.IsFragile,
            IsRestricted = Product.IsRestricted
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new UpdateAdminProductLogisticsRequest(
                Input.WeightKg,
                new AdminProductDimensions(Input.HeightCm, Input.WidthCm, Input.LengthCm),
                Input.IsFragile,
                Input.IsRestricted);

            await _client.UpdateProductLogisticsAsync(SkuId, request, cancellationToken);

            SuccessMessage = "Atributos logísticos atualizados com sucesso.";
            return RedirectToPage("/Admin/Products/Logistics", new { skuId = SkuId });
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    private async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        Product = await _client.GetProductAsync(SkuId, cancellationToken);
        return Product is not null;
    }

    public sealed class LogisticsInput
    {
        public decimal WeightKg { get; set; }
        public decimal HeightCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal LengthCm { get; set; }
        public bool IsFragile { get; set; }
        public bool IsRestricted { get; set; }
    }
}
