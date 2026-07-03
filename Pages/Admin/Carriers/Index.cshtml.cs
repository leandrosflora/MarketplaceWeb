using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Carriers;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public IndexModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty]
    public CreateCarrierInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminCarrierResponse> Carriers { get; private set; } = [];

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
            var request = new CreateAdminCarrierRequest(CreateInput.Code, CreateInput.Name, CreateInput.RequiresRealTimeValidation);
            var created = await _client.CreateCarrierAsync(request, cancellationToken);

            SuccessMessage = $"Carrier {created.Code} criado com sucesso.";
            return RedirectToPage("/Admin/Carriers/Index");
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(string carrierCode, string status, CancellationToken cancellationToken)
    {
        try
        {
            await _client.ChangeCarrierStatusAsync(carrierCode, new ChangeAdminCarrierStatusRequest(status, "admin change"), cancellationToken);
            SuccessMessage = $"Status de {carrierCode} alterado para {status}.";
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            Carriers = await _client.ListCarriersAsync(cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateCarrierInput
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool RequiresRealTimeValidation { get; set; }
    }
}
