using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Marketplace.Web.Infrastructure.Cart;
using Marketplace.Web.Pages.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using Xunit;

namespace MarketplaceWeb.Tests.Products;

public class DetailsModelAvailabilityTests
{
    private readonly IMarketplaceBffClient _bffClient = Substitute.For<IMarketplaceBffClient>();
    private readonly ICartOwnerIdAccessor _cartOwnerIdAccessor = Substitute.For<ICartOwnerIdAccessor>();

    private DetailsModel CreateModel(Guid skuId) => new(_bffClient, _cartOwnerIdAccessor)
    {
        PageContext = new PageContext { HttpContext = new DefaultHttpContext() },
        Id = skuId,
    };

    private static ProductPageResponse BuildPage(Guid skuId, bool availableForSale) => new(
        new ProductSummary(skuId, Guid.NewGuid(), "Produto Teste", "electronics", 100m, availableForSale),
        Shipping: null,
        Warnings: []);

    [Fact]
    public async Task OnPostAddToCartAsync_UnavailableProduct_DoesNotAddToCartAndRedisplaysPage()
    {
        var skuId = Guid.NewGuid();
        _bffClient.GetProductPageAsync(skuId, Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(BuildPage(skuId, availableForSale: false));

        var model = CreateModel(skuId);

        var result = await model.OnPostAddToCartAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        await _bffClient.DidNotReceive().AddCartItemAsync(
            Arg.Any<string>(), Arg.Any<AddCartItemRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnPostAddToCartAsync_AvailableProduct_AddsToCartAndRedirects()
    {
        var skuId = Guid.NewGuid();
        _bffClient.GetProductPageAsync(skuId, Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(BuildPage(skuId, availableForSale: true));
        _cartOwnerIdAccessor.GetOrCreateCartOwnerId(Arg.Any<HttpContext>()).Returns("cart-owner-1");
        _bffClient.AddCartItemAsync(Arg.Any<string>(), Arg.Any<AddCartItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CartResponse([], 1, 100m, false));

        var model = CreateModel(skuId);

        var result = await model.OnPostAddToCartAsync(CancellationToken.None);

        Assert.IsType<RedirectToPageResult>(result);
        await _bffClient.Received(1).AddCartItemAsync(
            "cart-owner-1", Arg.Any<AddCartItemRequest>(), Arg.Any<CancellationToken>());
    }
}
