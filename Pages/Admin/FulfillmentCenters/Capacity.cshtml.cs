using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.FulfillmentCenters;

public sealed class CapacityModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public CapacityModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty(SupportsGet = true)]
    public Guid FulfillmentCenterId { get; set; }

    [BindProperty]
    public ConfigureCapacityInput Input { get; set; } = new();

    public AdminFulfillmentCenterResponse? Center { get; private set; }

    public IReadOnlyList<AdminCapacitySlotResponse> Slots { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
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
            var request = new AdminConfigureCapacityRequest(Input.OperationDate, Input.Mode, Input.TotalCapacityUnits);
            await _client.ConfigureCapacityAsync(FulfillmentCenterId, request, cancellationToken);

            SuccessMessage = "Capacidade configurada com sucesso.";
            return RedirectToPage("/Admin/FulfillmentCenters/Capacity", new { fulfillmentCenterId = FulfillmentCenterId });
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
            Center = await _client.GetFulfillmentCenterAsync(FulfillmentCenterId, cancellationToken);
            Slots = await _client.GetCapacitySlotsAsync(FulfillmentCenterId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class ConfigureCapacityInput
    {
        public DateOnly OperationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public string Mode { get; set; } = "Fulfillment";
        public int TotalCapacityUnits { get; set; }
    }
}
