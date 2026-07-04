using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Marketplace.Web.Pages.Orders;

public sealed class IndexModel : PageModel
{
    private const int PageSize = 10;
    private static readonly Guid FallbackDemoBuyerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IMarketplaceBffClient _bffClient;
    private readonly Guid _demoBuyerId;

    public IndexModel(IMarketplaceBffClient bffClient, IConfiguration configuration)
    {
        _bffClient = bffClient;
        _demoBuyerId = Guid.TryParse(configuration["Checkout:DemoBuyerId"], out var configuredBuyerId)
            ? configuredBuyerId
            : FallbackDemoBuyerId;
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

        var orders = await _bffClient.ListOrdersAsync(_demoBuyerId, cancellationToken);
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
