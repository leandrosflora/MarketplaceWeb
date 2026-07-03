using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Inventory;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public IndexModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? SellerId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SkuId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? FulfillmentCenterId { get; set; }

    [BindProperty]
    public AdjustStockInput AdjustInput { get; set; } = new();

    public IReadOnlyList<AdminInventoryItemResponse> Items { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAdjustAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new AdminStockAdjustmentRequest(
                AdjustInput.SellerId,
                AdjustInput.SkuId,
                AdjustInput.FulfillmentCenterId,
                AdjustInput.QuantityDelta,
                AdjustInput.Reason,
                AdjustInput.Description);

            await _client.AdjustStockAsync(request, Guid.NewGuid().ToString("N"), cancellationToken);

            SuccessMessage = "Ajuste de estoque aplicado com sucesso.";
            return RedirectToPage("/Admin/Inventory/Index", new { sellerId = AdjustInput.SellerId, skuId = AdjustInput.SkuId, fulfillmentCenterId = AdjustInput.FulfillmentCenterId });
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Items = await _client.SearchInventoryAsync(SellerId, SkuId, FulfillmentCenterId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class AdjustStockInput
    {
        public Guid SellerId { get; set; }
        public Guid SkuId { get; set; }
        public Guid FulfillmentCenterId { get; set; }
        public int QuantityDelta { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
