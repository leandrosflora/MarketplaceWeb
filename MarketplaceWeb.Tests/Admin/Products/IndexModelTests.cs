using System.Net;
using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Marketplace.Web.Pages.Admin.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using Xunit;

namespace MarketplaceWeb.Tests.Admin.Products;

public class IndexModelTests
{
    private readonly IMarketplaceAdminBffClient _client = Substitute.For<IMarketplaceAdminBffClient>();

    private IndexModel CreateModel() => new(_client)
    {
        PageContext = new PageContext { HttpContext = new DefaultHttpContext() },
    };

    private static AdminProductResponse BuildProduct(string title, string status) => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        title,
        "electronics",
        10m,
        status,
        1m,
        10m,
        10m,
        10m,
        false,
        false);

    [Fact]
    public async Task OnGetAsync_PopulatesProductsFromListProductsAsync()
    {
        IReadOnlyList<AdminProductResponse> products =
            [BuildProduct("Ativo", "Active"), BuildProduct("Pausado", "Paused")];
        _client.ListProductsAsync(Arg.Any<CancellationToken>()).Returns(products);

        var model = CreateModel();

        await model.OnGetAsync(CancellationToken.None);

        Assert.Equal(2, model.Products.Count);
        Assert.Contains(model.Products, p => p.Status == "Paused");
    }

    [Fact]
    public async Task OnPostChangeStatusAsync_ChangesStatusAndReloadsFullList()
    {
        var skuId = Guid.NewGuid();
        IReadOnlyList<AdminProductResponse> products = [BuildProduct("Produto", "Paused")];
        _client.ListProductsAsync(Arg.Any<CancellationToken>()).Returns(products);

        var model = CreateModel();

        var result = await model.OnPostChangeStatusAsync(skuId, "Paused", CancellationToken.None);

        Assert.IsType<PageResult>(result);
        await _client.Received(1).ChangeProductStatusAsync(
            skuId, Arg.Is<ChangeAdminProductStatusRequest>(r => r.Status == "Paused"), Arg.Any<CancellationToken>());
        Assert.Single(model.Products);
        Assert.Null(model.ErrorMessage);
    }

    [Fact]
    public async Task OnPostChangeStatusAsync_WhenBffFails_SetsErrorMessageButStillReloadsList()
    {
        var skuId = Guid.NewGuid();
        _client.ChangeProductStatusAsync(
                skuId, Arg.Any<ChangeAdminProductStatusRequest>(), Arg.Any<CancellationToken>())
            .Returns<AdminProductResponse>(_ => throw new BffApiException(HttpStatusCode.ServiceUnavailable, "downstream unavailable"));
        _client.ListProductsAsync(Arg.Any<CancellationToken>()).Returns([BuildProduct("Produto", "Active")]);

        var model = CreateModel();

        var result = await model.OnPostChangeStatusAsync(skuId, "Blocked", CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("downstream unavailable", model.ErrorMessage);
        Assert.Single(model.Products);
    }
}
