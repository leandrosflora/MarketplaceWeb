using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Operations.Orders;

public sealed class IndexModel : PageModel
{
    private readonly IOrderVisibilityClient _client;

    public IndexModel(IOrderVisibilityClient client, IConfiguration configuration)
    {
        _client = client;
        JaegerBaseUrl = configuration["Jaeger:BaseUrl"] ?? "http://localhost:16686";
    }

    public string JaegerBaseUrl { get; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? HasError { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? StuckOnly { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? OrderId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CheckoutId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CorrelationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? BuyerId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SellerId { get; set; }

    public IReadOnlyList<OrderJourneySummary> Journeys { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    /// <summary>
    /// Polled by the page's inline JS every few seconds to refresh the table without a full
    /// reload. Same-origin, so it needs no SignalR/CORS setup — see design notes in
    /// docs/runbooks/order-visibility-local.md on why polling was used instead of the
    /// already-implemented SignalR hub for this first cut of the UI.
    /// </summary>
    public async Task<JsonResult> OnGetPollAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        return new JsonResult(Journeys);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var filter = new OrderJourneyListFilter(Status, HasError, StuckOnly, OrderId, CheckoutId, CorrelationId, BuyerId, SellerId);
            var result = await _client.ListJourneysAsync(filter, page: 1, pageSize: 100, cancellationToken);
            Journeys = result.Items;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Não foi possível carregar as jornadas: {ex.Message}";
        }
    }
}
