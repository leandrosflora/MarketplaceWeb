using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Network;

public sealed class LanesModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public LanesModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty]
    public CreateLaneInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminLogisticsLaneResponse> Lanes { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public bool ShowCreateModal { get; private set; }

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
            ShowCreateModal = true;
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            var schedules = new List<AdminLaneScheduleDto>();
            if (CreateInput.ScheduleDayOfWeek is { } day)
            {
                schedules.Add(new AdminLaneScheduleDto(day.ToString(), CreateInput.ScheduleDepartureTime, true));
            }

            var request = new CreateAdminLaneRequest(
                CreateInput.OriginNodeId,
                CreateInput.DestinationNodeId,
                CreateInput.CarrierCode,
                CreateInput.ServiceLevelCode,
                CreateInput.Mode,
                CreateInput.TransitMinutes,
                CreateInput.MaximumWeightKg,
                CreateInput.MaximumCubicWeightKg,
                CreateInput.SupportsFragileItems,
                CreateInput.SupportsRestrictedItems,
                CreateInput.Region,
                schedules);

            var created = await _client.CreateLaneAsync(request, cancellationToken);

            SuccessMessage = $"Lane {created.Id} criada com sucesso.";
            return RedirectToPage("/Admin/Network/Lanes");
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ShowCreateModal = true;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(Guid laneId, string status, string region, CancellationToken cancellationToken)
    {
        try
        {
            await _client.ChangeLaneStatusAsync(laneId, new ChangeAdminLaneStatusRequest(status, region), cancellationToken);
            SuccessMessage = $"Status da lane {laneId} alterado para {status}.";
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
            Lanes = await _client.ListLanesAsync(cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateLaneInput
    {
        public Guid OriginNodeId { get; set; }
        public Guid DestinationNodeId { get; set; }
        public string CarrierCode { get; set; } = string.Empty;
        public string ServiceLevelCode { get; set; } = "standard";
        public string Mode { get; set; } = "Road";
        public int TransitMinutes { get; set; }
        public decimal MaximumWeightKg { get; set; }
        public decimal MaximumCubicWeightKg { get; set; }
        public bool SupportsFragileItems { get; set; }
        public bool SupportsRestrictedItems { get; set; }
        public string Region { get; set; } = "Brasil Sudeste";
        public DayOfWeek? ScheduleDayOfWeek { get; set; }
        public TimeOnly ScheduleDepartureTime { get; set; } = new(9, 0);
    }
}
