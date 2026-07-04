using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarketplaceWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMarketplaceBffClient _bffClient;

        public IndexModel(IMarketplaceBffClient bffClient)
        {
            _bffClient = bffClient;
        }

        public IReadOnlyList<ProductSearchItem> Products { get; private set; } = [];

        public string? ErrorMessage { get; private set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _bffClient.SearchProductsAsync(string.Empty, null, null, null, null, cancellationToken);
                Products = response.Products ?? [];
            }
            catch (BffApiException ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
