using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Pricing;

public sealed class RateCardsModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public RateCardsModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty]
    public CreateRateCardInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminRateCardResponse> RateCards { get; private set; } = [];

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
            var band = new AdminRateBandRequest(
                CreateInput.OriginNodeId,
                CreateInput.DestinationZone,
                CreateInput.MinimumWeightKg,
                CreateInput.MaximumWeightKg,
                CreateInput.BasePrice,
                CreateInput.IncludedWeightKg,
                CreateInput.WeightIncrementKg,
                CreateInput.PricePerWeightIncrement,
                CreateInput.FuelSurchargePercentage,
                CreateInput.RemoteAreaFee,
                CreateInput.FragileFee,
                CreateInput.OversizeThresholdKg,
                CreateInput.OversizeFee,
                CreateInput.MinimumLogisticsCost);

            var request = new AdminRateCardRequest(
                CreateInput.Code,
                CreateInput.CarrierCode,
                CreateInput.ServiceLevelCode,
                CreateInput.Currency,
                CreateInput.EffectiveFrom,
                CreateInput.EffectiveUntil,
                [band]);

            var created = await _client.CreateRateCardAsync(request, cancellationToken);

            SuccessMessage = $"Rate card {created.Code} criado com sucesso.";
            return RedirectToPage("/Admin/Pricing/RateCards");
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid rateCardId, CancellationToken cancellationToken)
    {
        try
        {
            await _client.ActivateRateCardAsync(rateCardId, cancellationToken);
            SuccessMessage = "Rate card ativado.";
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }

        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostRetireAsync(Guid rateCardId, CancellationToken cancellationToken)
    {
        try
        {
            await _client.RetireRateCardAsync(rateCardId, cancellationToken);
            SuccessMessage = "Rate card retirado.";
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
            RateCards = await _client.ListRateCardsAsync(cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateRateCardInput
    {
        public string Code { get; set; } = string.Empty;
        public string CarrierCode { get; set; } = string.Empty;
        public string ServiceLevelCode { get; set; } = string.Empty;
        public string Currency { get; set; } = "BRL";
        public DateTimeOffset EffectiveFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset EffectiveUntil { get; set; } = DateTimeOffset.UtcNow.AddYears(1);
        public Guid OriginNodeId { get; set; }
        public string DestinationZone { get; set; } = string.Empty;
        public decimal MinimumWeightKg { get; set; }
        public decimal MaximumWeightKg { get; set; } = 30;
        public decimal BasePrice { get; set; }
        public decimal IncludedWeightKg { get; set; } = 1;
        public decimal WeightIncrementKg { get; set; } = 1;
        public decimal PricePerWeightIncrement { get; set; }
        public decimal FuelSurchargePercentage { get; set; }
        public decimal RemoteAreaFee { get; set; }
        public decimal FragileFee { get; set; }
        public decimal OversizeThresholdKg { get; set; } = 30;
        public decimal OversizeFee { get; set; }
        public decimal MinimumLogisticsCost { get; set; }
    }
}
