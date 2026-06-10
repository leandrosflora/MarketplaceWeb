using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages;

public sealed class SearchModel : PageModel
{
    private const int MaximumQueryLength = 100;

    private readonly IMarketplaceBffClient _bffClient;

    public SearchModel(IMarketplaceBffClient bffClient)
    {
        _bffClient = bffClient;
    }

    public string? Query { get; private set; }

    public IReadOnlyList<ProductSearchItem> Products { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(string? query, CancellationToken cancellationToken)
    {
        Query = NormalizeQuery(query);

        if (Query is null)
        {
            return;
        }

        try
        {
            var response = await _bffClient.SearchProductsAsync(Query, cancellationToken);
            Products = response.Products ?? [];
        }
        catch (BffApiException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private static string? NormalizeQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        var normalizedQuery = query.Trim();

        return normalizedQuery.Length <= MaximumQueryLength
            ? normalizedQuery
            : normalizedQuery[..MaximumQueryLength];
    }
}
