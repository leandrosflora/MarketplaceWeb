using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.FulfillmentCenters;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public IndexModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty]
    public CreateCenterInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminFulfillmentCenterResponse> Centers { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new CreateAdminFulfillmentCenterRequest(
                CreateInput.Code,
                CreateInput.Name,
                CreateInput.Region,
                CreateInput.TimeZoneId,
                CreateInput.MaximumWeightKg,
                CreateInput.MaximumCubicWeightKg,
                CreateInput.SupportsFragileItems,
                CreateInput.SupportsRestrictedItems);

            var created = await _client.CreateFulfillmentCenterAsync(request, cancellationToken);

            SuccessMessage = $"Fulfillment center {created.Code} criado com sucesso.";
            return RedirectToPage("/Admin/FulfillmentCenters/Index");
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(Guid fulfillmentCenterId, string status, CancellationToken cancellationToken)
    {
        try
        {
            await _client.ChangeFulfillmentCenterStatusAsync(fulfillmentCenterId, new ChangeAdminFulfillmentCenterStatusRequest(status), cancellationToken);
            SuccessMessage = $"Status de {fulfillmentCenterId} alterado para {status}.";
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Centers = await _client.ListFulfillmentCentersAsync(cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateCenterInput
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string TimeZoneId { get; set; } = "America/Sao_Paulo";
        public decimal MaximumWeightKg { get; set; }
        public decimal MaximumCubicWeightKg { get; set; }
        public bool SupportsFragileItems { get; set; }
        public bool SupportsRestrictedItems { get; set; }
    }
}
