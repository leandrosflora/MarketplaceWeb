using System.ComponentModel.DataAnnotations;
using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Checkout;

public sealed class PaymentModel : PageModel
{
    private readonly IMarketplaceBffClient _bffClient;

    public PaymentModel(IMarketplaceBffClient bffClient)
    {
        _bffClient = bffClient;
    }

    [BindProperty(SupportsGet = true)]
    public Guid CheckoutId { get; set; }

    [BindProperty]
    public PaymentMethodInput Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var checkout = await _bffClient.GetCheckoutAsync(CheckoutId, cancellationToken);

        return checkout is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _bffClient.SubmitPaymentMethodAsync(
                CheckoutId,
                new PaymentMethodRequest(Input.CardholderName, Input.MaskedCardNumber, Input.ExpiryMonthYear),
                cancellationToken);

            return RedirectToPage("/Checkout/Review", new { checkoutId = CheckoutId });
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
            return Page();
        }
    }

    public sealed class PaymentMethodInput
    {
        [Required(ErrorMessage = "Informe o nome do titular do cartão.")]
        [Display(Name = "Nome do titular")]
        public string CardholderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o número do cartão.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Informe os últimos 4 dígitos do cartão.")]
        [Display(Name = "Últimos 4 dígitos do cartão")]
        public string MaskedCardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a validade do cartão.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use o formato MM/AA.")]
        [Display(Name = "Validade (MM/AA)")]
        public string ExpiryMonthYear { get; set; } = string.Empty;
    }
}
