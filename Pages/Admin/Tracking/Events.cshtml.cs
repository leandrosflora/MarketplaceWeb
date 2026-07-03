using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Tracking;

public sealed class EventsModel : PageModel
{
    private static readonly string[] TrackingStatuses =
    [
        "Created", "LabelGenerated", "ReadyForPickup", "PickedUp", "InTransit",
        "AtDistributionCenter", "OutForDelivery", "DeliveryAttempted", "Delivered",
        "Exception", "Cancelled", "Returned"
    ];

    private readonly IMarketplaceAdminBffClient _client;

    public EventsModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty]
    public CreateEventInput Input { get; set; } = new();

    public IReadOnlyList<string> StatusOptions => TrackingStatuses;

    public AdminShipmentTrackingResponse? CurrentTracking { get; private set; }

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(Guid? shipmentId, Guid? orderId, CancellationToken cancellationToken)
    {
        if (shipmentId is { } id)
        {
            Input.ShipmentId = id;
        }

        if (orderId is { } order)
        {
            Input.OrderId = order;
        }

        if (shipmentId is { } sid)
        {
            await LoadTrackingAsync(sid, cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var location = string.IsNullOrWhiteSpace(Input.City) && string.IsNullOrWhiteSpace(Input.State)
                ? null
                : new AdminTrackingLocationDto(null, Input.City, Input.State, Input.Country);

            var request = new CreateAdminTrackingEventRequest(
                Input.ShipmentId,
                Input.OrderId,
                Input.BuyerId,
                Input.TrackingCode,
                Input.CarrierCode,
                Input.Status,
                Input.Description,
                null,
                location,
                new DateTimeOffset(Input.OccurredAt, TimeSpan.Zero),
                null);

            await _client.CreateTrackingEventAsync(request, cancellationToken);

            SuccessMessage = "Evento de tracking registrado com sucesso.";
            return RedirectToPage("/Admin/Tracking/Events", new { shipmentId = Input.ShipmentId, orderId = Input.OrderId });
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
            await LoadTrackingAsync(Input.ShipmentId, cancellationToken);
            return Page();
        }
    }

    private async Task LoadTrackingAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        try
        {
            CurrentTracking = await _client.GetShipmentTrackingAsync(shipmentId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateEventInput
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid BuyerId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string CarrierCode { get; set; } = string.Empty;
        public string Status { get; set; } = "InTransit";
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string Country { get; set; } = "BR";
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
