using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Marketplace.Web.Pages.Checkout;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;

namespace MarketplaceWeb.Tests.Checkout;

public sealed class CheckoutPaymentFlowTests
{
    [Fact]
    public async Task Payment_OnPostAsync_ValidInput_SubmitsAndRedirectsToReview()
    {
        var bffClient = Substitute.For<IMarketplaceBffClient>();
        var checkoutId = Guid.NewGuid();
        bffClient.SubmitPaymentMethodAsync(checkoutId, Arg.Any<PaymentMethodRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentMethodResponse(checkoutId, "pi_mock_123"));

        var model = new PaymentModel(bffClient)
        {
            CheckoutId = checkoutId,
            Input = new PaymentModel.PaymentMethodInput
            {
                CardholderName = "Joao Silva",
                MaskedCardNumber = "1234",
                ExpiryMonthYear = "12/29"
            },
            PageContext = CreatePageContext()
        };

        var result = await model.OnPostAsync(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Checkout/Review", redirect.PageName);
        await bffClient.Received(1).SubmitPaymentMethodAsync(checkoutId, Arg.Any<PaymentMethodRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Payment_OnPostAsync_InvalidModelState_ReturnsPageWithoutCallingBff()
    {
        var bffClient = Substitute.For<IMarketplaceBffClient>();
        var model = new PaymentModel(bffClient)
        {
            CheckoutId = Guid.NewGuid(),
            PageContext = CreatePageContext()
        };
        model.ModelState.AddModelError("Input.CardholderName", "Informe o nome do titular do cartão.");

        var result = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        await bffClient.DidNotReceiveWithAnyArgs().SubmitPaymentMethodAsync(default, default!, default);
    }

    [Fact]
    public async Task Review_OnGetAsync_NoPaymentMethod_BlocksConfirm()
    {
        var bffClient = Substitute.For<IMarketplaceBffClient>();
        var checkoutId = Guid.NewGuid();
        bffClient.GetCheckoutAsync(checkoutId, Arg.Any<CancellationToken>())
            .Returns(new CheckoutPageResponse(checkoutId, 100m, 10m, 110m, "BRL", new ShippingOptionResponse(null, null, null, null, 10m), []));
        bffClient.GetPaymentMethodAsync(checkoutId, Arg.Any<CancellationToken>())
            .Returns((PaymentMethodResponse?)null);

        var model = new ReviewModel(bffClient)
        {
            CheckoutId = checkoutId,
            PageContext = CreatePageContext()
        };

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.False(model.HasPaymentMethod);
        Assert.Equal(string.Empty, model.Input.PaymentIntentId);
    }

    [Fact]
    public async Task Review_OnGetAsync_WithPaymentMethod_EnablesConfirm()
    {
        var bffClient = Substitute.For<IMarketplaceBffClient>();
        var checkoutId = Guid.NewGuid();
        bffClient.GetCheckoutAsync(checkoutId, Arg.Any<CancellationToken>())
            .Returns(new CheckoutPageResponse(checkoutId, 100m, 10m, 110m, "BRL", new ShippingOptionResponse(null, null, null, null, 10m), []));
        bffClient.GetPaymentMethodAsync(checkoutId, Arg.Any<CancellationToken>())
            .Returns(new PaymentMethodResponse(checkoutId, "pi_mock_abc"));

        var model = new ReviewModel(bffClient)
        {
            CheckoutId = checkoutId,
            PageContext = CreatePageContext()
        };

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.True(model.HasPaymentMethod);
        Assert.Equal("pi_mock_abc", model.Input.PaymentIntentId);
    }

    private static PageContext CreatePageContext()
    {
        return new PageContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}
