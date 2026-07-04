using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Operations.Orders;

public sealed class DetailModel : PageModel
{
    private readonly IOrderVisibilityClient _client;

    public DetailModel(IOrderVisibilityClient client, IConfiguration configuration)
    {
        _client = client;
        JaegerBaseUrl = configuration["Jaeger:BaseUrl"] ?? "http://localhost:16686";
    }

    [BindProperty(SupportsGet = true)]
    public Guid OrderId { get; set; }

    public string JaegerBaseUrl { get; }

    public OrderJourneySummary? Journey { get; private set; }

    public IReadOnlyList<OrderJourneyEventDetail> Events { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Journey = await _client.GetByOrderIdAsync(OrderId, cancellationToken);
            if (Journey is null)
            {
                ErrorMessage = "Jornada não encontrada para este orderId.";
                return;
            }

            Events = await _client.GetEventsByOrderIdAsync(OrderId, cancellationToken) ?? [];
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Não foi possível carregar a jornada: {ex.Message}";
        }
    }

    public string TraceUrl(string? traceId, string correlationId) =>
        string.IsNullOrEmpty(traceId)
            ? $"{JaegerBaseUrl}/search?tags={Uri.EscapeDataString($$"""{"correlation.id":"{{correlationId}}"}""")}"
            : $"{JaegerBaseUrl}/trace/{traceId}";
}
