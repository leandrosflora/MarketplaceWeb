using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Products;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public IndexModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? SkuId { get; set; }

    [BindProperty]
    public CreateProductInput CreateInput { get; set; } = new();

    public AdminProductResponse? Product { get; private set; }

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (SkuId is { } skuId)
        {
            try
            {
                Product = await _client.GetProductAsync(skuId, cancellationToken);

                if (Product is null)
                {
                    ErrorMessage = $"Nenhum produto encontrado para o SKU {skuId}.";
                }
            }
            catch (BffApiException exception)
            {
                ErrorMessage = exception.Message;
            }
        }
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new CreateAdminProductRequest(
                CreateInput.SellerId,
                CreateInput.SkuId,
                CreateInput.Title,
                CreateInput.Category,
                CreateInput.Price,
                new AdminProductDimensions(CreateInput.HeightCm, CreateInput.WidthCm, CreateInput.LengthCm),
                CreateInput.WeightKg,
                CreateInput.IsFragile,
                CreateInput.IsRestricted);

            var created = await _client.CreateProductAsync(request, cancellationToken);

            SuccessMessage = $"Produto {created.SkuId} criado com sucesso.";
            return RedirectToPage("/Admin/Products/Index", new { skuId = created.SkuId });
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(Guid skuId, string status, CancellationToken cancellationToken)
    {
        try
        {
            await _client.ChangeProductStatusAsync(skuId, new ChangeAdminProductStatusRequest(status), cancellationToken);

            SuccessMessage = $"Status do produto {skuId} alterado para {status}.";
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        SkuId = skuId;

        try
        {
            Product = await _client.GetProductAsync(skuId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage ??= exception.Message;
        }

        return Page();
    }

    public sealed class CreateProductInput
    {
        public Guid SellerId { get; set; }
        public Guid SkuId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal WeightKg { get; set; }
        public decimal HeightCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal LengthCm { get; set; }
        public bool IsFragile { get; set; }
        public bool IsRestricted { get; set; }
    }
}
