using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Network;

public sealed class NodesModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public NodesModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty(SupportsGet = true)]
    public string? Region { get; set; }

    [BindProperty]
    public CreateNodeInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminLogisticsNodeResponse> Nodes { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new CreateAdminNodeRequest(
                CreateInput.Code,
                CreateInput.Name,
                CreateInput.Region,
                CreateInput.TimeZoneId,
                CreateInput.Type,
                CreateInput.HandlingMinutes);

            var created = await _client.CreateNodeAsync(request, cancellationToken);

            SuccessMessage = $"Nó {created.Code} criado com sucesso.";
            return RedirectToPage("/Admin/Network/Nodes");
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Nodes = await _client.ListNodesAsync(Region, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateNodeInput
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = "Brasil Sudeste";
        public string TimeZoneId { get; set; } = "America/Sao_Paulo";
        public string Type { get; set; } = "RegionalHub";
        public int HandlingMinutes { get; set; }
    }
}
