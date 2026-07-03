using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Carriers;

public sealed class ServiceLevelsModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public ServiceLevelsModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty(SupportsGet = true)]
    public string CarrierCode { get; set; } = string.Empty;

    [BindProperty]
    public CreateServiceLevelInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminServiceLevelResponse> ServiceLevels { get; private set; } = [];

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
            var request = new CreateAdminServiceLevelRequest(
                CreateInput.Code,
                CreateInput.Name,
                CreateInput.Mode,
                CreateInput.MaximumWeightKg,
                CreateInput.MaximumCubicWeightKg,
                CreateInput.SupportsFragileItems,
                CreateInput.SupportsRestrictedItems,
                CreateInput.Priority);

            await _client.CreateServiceLevelAsync(CarrierCode, request, cancellationToken);

            SuccessMessage = $"Service level {CreateInput.Code} criado com sucesso.";
            return RedirectToPage("/Admin/Carriers/ServiceLevels", new { carrierCode = CarrierCode });
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
            ServiceLevels = await _client.ListServiceLevelsAsync(CarrierCode, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateServiceLevelInput
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Mode { get; set; } = "FULFILLMENT";
        public decimal MaximumWeightKg { get; set; }
        public decimal MaximumCubicWeightKg { get; set; }
        public bool SupportsFragileItems { get; set; }
        public bool SupportsRestrictedItems { get; set; }
        public int Priority { get; set; } = 1;
    }
}
