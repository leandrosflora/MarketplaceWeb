using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Orders;

public sealed class IndexModel : PageModel
{
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

    public IReadOnlyList<OrderListItemResponse> Orders { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Orders = await _bffClient.ListOrdersAsync(_demoBuyerId, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || OrderId is null)
        {
            ModelState.AddModelError(string.Empty, "Informe um identificador de pedido válido.");
            Orders = await _bffClient.ListOrdersAsync(_demoBuyerId, cancellationToken);

            return Page();
        }

        return RedirectToPage("/Orders/Details", new { id = OrderId });
    }
}
