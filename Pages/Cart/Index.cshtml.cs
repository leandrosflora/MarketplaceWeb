using System.ComponentModel.DataAnnotations;
using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Marketplace.Web.Infrastructure.Auth;
using Marketplace.Web.Infrastructure.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Cart;

public sealed class IndexModel : PageModel
{
    private readonly IMarketplaceBffClient _bffClient;
    private readonly ICartOwnerIdAccessor _cartOwnerIdAccessor;

    public IndexModel(IMarketplaceBffClient bffClient, ICartOwnerIdAccessor cartOwnerIdAccessor)
    {
        _bffClient = bffClient;
        _cartOwnerIdAccessor = cartOwnerIdAccessor;
    }

    [BindProperty]
    [Required(ErrorMessage = "Informe um CEP válido para continuar.")]
    public string? ZipCode { get; set; }

    public CartResponse CartState { get; private set; } = new([], 0, 0m, false);

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCartAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostUpdateQuantityAsync(Guid skuId, int quantity, CancellationToken cancellationToken)
    {
        var cartOwnerId = _cartOwnerIdAccessor.GetOrCreateCartOwnerId(HttpContext);

        try
        {
            await _bffClient.UpdateCartItemQuantityAsync(cartOwnerId, skuId, quantity, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid skuId, CancellationToken cancellationToken)
    {
        var cartOwnerId = _cartOwnerIdAccessor.GetOrCreateCartOwnerId(HttpContext);

        try
        {
            await _bffClient.RemoveCartItemAsync(cartOwnerId, skuId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostProceedToCheckoutAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        var buyerIdClaim = User.FindFirst(MarketplaceAuthConstants.BuyerIdClaim)?.Value;

        if (!Guid.TryParse(buyerIdClaim, out var buyerId))
        {
            ErrorMessage = "Sua conta não está habilitada para realizar compras.";
            await LoadCartAsync(cancellationToken);
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadCartAsync(cancellationToken);
            return Page();
        }

        var normalizedZipCode = new string((ZipCode ?? string.Empty).Where(char.IsDigit).ToArray());

        if (normalizedZipCode.Length != 8)
        {
            ModelState.AddModelError(nameof(ZipCode), "Informe um CEP válido (8 dígitos) para continuar.");
            await LoadCartAsync(cancellationToken);
            return Page();
        }

        var cartOwnerId = _cartOwnerIdAccessor.GetOrCreateCartOwnerId(HttpContext);

        try
        {
            var response = await _bffClient.ProceedToCheckoutAsync(
                cartOwnerId,
                new ProceedToCheckoutRequest(buyerId, normalizedZipCode),
                cancellationToken);

            if (response.CheckoutIds.Count == 0)
            {
                ErrorMessage = "Não foi possível criar o checkout.";
                await LoadCartAsync(cancellationToken);
                return Page();
            }

            if (response.CheckoutIds.Count > 1)
            {
                SuccessMessage = $"Seu carrinho tinha itens de {response.CheckoutIds.Count} vendedores diferentes; foram criados {response.CheckoutIds.Count} pedidos separados.";
            }

            return RedirectToPage("/Checkout/Payment", new { checkoutId = response.CheckoutIds[0] });
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
            await LoadCartAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadCartAsync(CancellationToken cancellationToken)
    {
        var cartOwnerId = _cartOwnerIdAccessor.GetOrCreateCartOwnerId(HttpContext);

        try
        {
            CartState = await _bffClient.GetCartAsync(cartOwnerId, cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }
}
