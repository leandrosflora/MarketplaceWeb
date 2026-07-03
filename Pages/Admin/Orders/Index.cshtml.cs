using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Orders;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public IndexModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    public Guid? BuyerId { get; set; }
    public Guid? SellerId { get; set; }
    public string? Status { get; set; }

    public IReadOnlyList<AdminOrderListItemResponse> Orders { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(Guid? buyerId, Guid? sellerId, string? status, CancellationToken cancellationToken)
    {
        BuyerId = buyerId;
        SellerId = sellerId;
        Status = status;

        try
        {
            Orders = await _client.ListOrdersAsync(buyerId, sellerId, status, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }
}
