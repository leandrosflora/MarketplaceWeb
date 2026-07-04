using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MarketplaceWeb.Pages
{
    public class IndexModel : PageModel
    {
        private const int PageSize = 10;
        private const int FeaturedProductCount = 5;

        private readonly IMarketplaceBffClient _bffClient;

        public IndexModel(IMarketplaceBffClient bffClient)
        {
            _bffClient = bffClient;
        }

        public IReadOnlyList<ProductSearchItem> Products { get; private set; } = [];

        public IReadOnlyList<ProductSearchItem> FeaturedProducts { get; private set; } = [];

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalCount { get; private set; }

        public int TotalPages { get; private set; } = 1;

        public string? ErrorMessage { get; private set; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            await LoadProductsAsync(cancellationToken);
            await LoadFeaturedProductsAsync(cancellationToken);
        }

        public async Task<IActionResult> OnGetProductsAsync(CancellationToken cancellationToken)
        {
            await LoadProductsAsync(cancellationToken);

            return Partial("_ProductGridPartial", this);
        }

        private async Task LoadProductsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (PageNumber < 1)
                {
                    PageNumber = 1;
                }

                var response = await _bffClient.SearchProductsAsync(
                    string.Empty,
                    PageNumber,
                    PageSize,
                    null,
                    null,
                    cancellationToken);

                Products = response.Products ?? [];
                TotalCount = response.TotalItems ?? Products.Count;
                TotalPages = TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

                if (PageNumber > TotalPages)
                {
                    PageNumber = TotalPages;
                }
            }
            catch (BffApiException ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task LoadFeaturedProductsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _bffClient.SearchProductsAsync(
                    string.Empty,
                    1,
                    50,
                    null,
                    null,
                    cancellationToken);

                var all = response.Products ?? [];

                FeaturedProducts = all.Count <= FeaturedProductCount
                    ? all
                    : all.OrderBy(_ => Random.Shared.Next()).Take(FeaturedProductCount).ToList();
            }
            catch (BffApiException)
            {
                FeaturedProducts = [];
            }
        }

        public string PageUrl(int page)
        {
            var query = QueryHelpers.ParseQuery(Request.QueryString.Value ?? string.Empty)
                .ToDictionary(kv => kv.Key, kv => (string?)kv.Value.ToString());
            query.Remove("handler");
            query["PageNumber"] = page.ToString();

            return QueryHelpers.AddQueryString(Request.Path.Value ?? "/", query);
        }

        public string AjaxPageUrl(int page)
        {
            var query = QueryHelpers.ParseQuery(Request.QueryString.Value ?? string.Empty)
                .ToDictionary(kv => kv.Key, kv => (string?)kv.Value.ToString());
            query["PageNumber"] = page.ToString();
            query["handler"] = "Products";

            return QueryHelpers.AddQueryString(Request.Path.Value ?? "/", query);
        }
    }
}
