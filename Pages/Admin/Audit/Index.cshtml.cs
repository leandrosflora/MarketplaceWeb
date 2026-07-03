using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Audit;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public IndexModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public string? AdminUserId { get; set; }

    public IReadOnlyList<AdminAuditLogEntryResponse> Entries { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(string? entityType, string? action, string? adminUserId, CancellationToken cancellationToken)
    {
        EntityType = entityType;
        Action = action;
        AdminUserId = adminUserId;

        try
        {
            Entries = await _client.ListAuditLogAsync(entityType, action, adminUserId, 200, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }
}
