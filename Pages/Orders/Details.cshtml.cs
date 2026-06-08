using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Orders;

public sealed class DetailsModel : PageModel
{
    private readonly IMarketplaceBffClient _bffClient;

    public DetailsModel(IMarketplaceBffClient bffClient)
    {
        _bffClient = bffClient;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public string CancellationReason { get; set; } = "Cancelado pelo comprador";

    public OrderPageResponse OrderPage { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        return await LoadAsync(cancellationToken) ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken cancellationToken)
    {
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        try
        {
            await _bffClient.CancelOrderAsync(
                Id,
                CancellationReason,
                idempotencyKey,
                cancellationToken);

            TempData["SuccessMessage"] = "Cancelamento solicitado.";

            return RedirectToPage(new { id = Id });
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);

            return Page();
        }
    }

    private async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        var response = await _bffClient.GetOrderAsync(Id, cancellationToken);

        if (response is null)
        {
            return false;
        }

        OrderPage = response;

        return true;
    }
}
