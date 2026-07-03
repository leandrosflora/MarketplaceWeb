using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Simulators;

public sealed class ShippingPromiseModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public ShippingPromiseModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty]
    public SimulateInput Input { get; set; } = new();

    public AdminShippingPromiseSimulationResponse? Result { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new SimulateAdminShippingPromiseRequest(
                null,
                Input.BuyerId,
                Input.SellerId,
                new SimulateAdminAddressRequest(Input.DestinationZipCode, Input.City, Input.State, Input.Country),
                [new SimulateAdminShippingPromiseItemRequest(Input.SkuId, Input.Quantity, Input.UnitPrice)]);

            Result = await _client.SimulateShippingPromiseAsync(request, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        return Page();
    }

    public sealed class SimulateInput
    {
        public Guid SkuId { get; set; }
        public Guid SellerId { get; set; }
        public Guid BuyerId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public string DestinationZipCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "BR";
    }
}
