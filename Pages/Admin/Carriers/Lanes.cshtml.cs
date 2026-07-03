using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Carriers;

public sealed class LanesModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public LanesModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty(SupportsGet = true)]
    public string CarrierCode { get; set; } = string.Empty;

    [BindProperty]
    public CreateLaneInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminCarrierLaneResponse> Lanes { get; private set; } = [];

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
            var days = new HashSet<string>();
            if (CreateInput.OperatingDays is not null)
            {
                foreach (var day in CreateInput.OperatingDays)
                {
                    days.Add(day);
                }
            }

            var request = new CreateAdminCarrierLaneRequest(
                CreateInput.ServiceLevelCode,
                CreateInput.OriginNodeId,
                CreateInput.DestinationNodeId,
                CreateInput.TimeZoneId,
                CreateInput.CutoffTime,
                days);

            await _client.CreateCarrierLaneAsync(CarrierCode, request, cancellationToken);

            SuccessMessage = "Lane criada com sucesso.";
            return RedirectToPage("/Admin/Carriers/Lanes", new { carrierCode = CarrierCode });
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
            Lanes = await _client.ListCarrierLanesAsync(CarrierCode, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateLaneInput
    {
        public string ServiceLevelCode { get; set; } = string.Empty;
        public Guid OriginNodeId { get; set; }
        public Guid DestinationNodeId { get; set; }
        public string TimeZoneId { get; set; } = "America/Sao_Paulo";
        public string CutoffTime { get; set; } = "14:00";
        public string[]? OperatingDays { get; set; }
    }
}
