using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Orders;

public sealed class DetailModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public DetailModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    public Guid OrderId { get; private set; }

    public AdminOrderResponse? Order { get; private set; }

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public string CancelReason { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        OrderId = orderId;
        await LoadAsync(cancellationToken);

        return Order is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid orderId, CancellationToken cancellationToken)
    {
        OrderId = orderId;

        try
        {
            var request = new CancelAdminOrderRequest(CancelReason);
            await _client.CancelOrderAsync(orderId, request, Guid.NewGuid().ToString("N"), cancellationToken);

            SuccessMessage = "Pedido cancelado com sucesso.";
            return RedirectToPage("/Admin/Orders/Detail", new { orderId });
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Order = await _client.GetOrderAsync(OrderId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }
}
