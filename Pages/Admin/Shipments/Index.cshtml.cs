using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Shipments;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public IndexModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    public Guid? OrderId { get; set; }

    public IReadOnlyList<AdminShipmentListItemResponse> Shipments { get; private set; } = [];

    public AdminShipmentResponse? SelectedShipment { get; private set; }

    public string? LabelUrl { get; private set; }

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(Guid? orderId, Guid? shipmentId, CancellationToken cancellationToken)
    {
        OrderId = orderId;
        await LoadAsync(cancellationToken);

        if (shipmentId is { } id)
        {
            await LoadDetailAsync(id, cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid shipmentId, Guid? orderId, CancellationToken cancellationToken)
    {
        OrderId = orderId;

        try
        {
            await _client.CancelShipmentAsync(shipmentId, Guid.NewGuid().ToString("N"), cancellationToken);
            SuccessMessage = "Shipment cancelado com sucesso.";
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        return RedirectToPage("/Admin/Shipments/Index", new { orderId });
    }

    public async Task<IActionResult> OnGetLabelAsync(Guid shipmentId, Guid? orderId, CancellationToken cancellationToken)
    {
        OrderId = orderId;
        await LoadAsync(cancellationToken);
        await LoadDetailAsync(shipmentId, cancellationToken);

        try
        {
            var label = await _client.GetShipmentLabelAsync(shipmentId, cancellationToken);
            LabelUrl = label?.Url;
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Shipments = await _client.ListShipmentsAsync(OrderId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    private async Task LoadDetailAsync(Guid shipmentId, CancellationToken cancellationToken)
    {
        try
        {
            SelectedShipment = await _client.GetShipmentAsync(shipmentId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }
}
