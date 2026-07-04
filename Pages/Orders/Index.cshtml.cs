using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Marketplace.Web.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Marketplace.Web.Pages.Orders;

public sealed class IndexModel : PageModel
{
    private const int PageSize = 10;

    private readonly IMarketplaceBffClient _bffClient;

    public IndexModel(IMarketplaceBffClient bffClient)
    {
        _bffClient = bffClient;
    }

    [BindProperty]
    public Guid? OrderId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<OrderListItemResponse> PagedOrders { get; private set; } = [];

    public int TotalCount { get; private set; }

    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadOrdersAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || OrderId is null)
        {
            ModelState.AddModelError(string.Empty, "Informe um identificador de pedido valido.");
            await LoadOrdersAsync(cancellationToken);

            return Page();
        }

        return RedirectToPage("/Orders/Details", new { id = OrderId });
    }

    public string PageUrl(int page)
    {
        var query = QueryHelpers.ParseQuery(Request.QueryString.Value ?? string.Empty)
            .ToDictionary(kv => kv.Key, kv => (string?)kv.Value.ToString());
        query["PageNumber"] = page.ToString();

        return QueryHelpers.AddQueryString(Request.Path.Value ?? "/Orders", query);
    }

    private async Task LoadOrdersAsync(CancellationToken cancellationToken)
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        var buyerIdClaim = User.FindFirst(MarketplaceAuthConstants.BuyerIdClaim)?.Value;

        if (!Guid.TryParse(buyerIdClaim, out var buyerId))
        {
            PagedOrders = [];
            TotalCount = 0;
            return;
        }

        var orders = await _bffClient.ListOrdersAsync(buyerId, cancellationToken);
        TotalCount = orders.Count;

        if (PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }

        PagedOrders = orders
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
