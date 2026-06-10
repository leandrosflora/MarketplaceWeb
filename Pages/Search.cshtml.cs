using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages;

[AllowAnonymous]
public sealed class SearchModel : PageModel
{
    private const int MaximumQueryLength = 100;

    public string? Query { get; private set; }

    public void OnGet(string? query)
    {
        Query = NormalizeQuery(query);
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
