using Marketplace.Web.Clients;
using Marketplace.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marketplace.Web.Pages.Admin.Pricing;

public sealed class SubsidyRulesModel : PageModel
{
    private readonly IMarketplaceAdminBffClient _client;

    public SubsidyRulesModel(IMarketplaceAdminBffClient client)
    {
        _client = client;
    }

    [BindProperty]
    public CreateSubsidyRuleInput CreateInput { get; set; } = new();

    public IReadOnlyList<AdminPromotionRuleResponse> Rules { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public bool ShowCreateModal { get; private set; }

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
            ShowCreateModal = true;
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            var request = new CreateAdminPromotionRuleRequest(
                CreateInput.Code,
                CreateInput.SellerId,
                CreateInput.Priority,
                CreateInput.MinimumCartTotal,
                CreateInput.CustomerDiscountPercentage,
                CreateInput.PlatformSubsidyPercentage,
                CreateInput.SellerSubsidyPercentage,
                CreateInput.MaximumBenefit,
                CreateInput.StartsAt,
                CreateInput.EndsAt);

            var created = await _client.CreateSubsidyRuleAsync(request, cancellationToken);

            SuccessMessage = $"Regra {created.Code} criada com sucesso.";
            return RedirectToPage("/Admin/Pricing/SubsidyRules");
        }
        catch (BffApiException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ShowCreateModal = true;
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid promotionId, bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            await _client.ChangeSubsidyRuleActiveAsync(promotionId, isActive, cancellationToken);
            SuccessMessage = "Status atualizado.";
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
            Rules = await _client.ListSubsidyRulesAsync(cancellationToken);
        }
        catch (BffApiException exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public sealed class CreateSubsidyRuleInput
    {
        public string Code { get; set; } = string.Empty;
        public Guid? SellerId { get; set; }
        public int Priority { get; set; } = 1;
        public decimal MinimumCartTotal { get; set; }
        public decimal CustomerDiscountPercentage { get; set; }
        public decimal PlatformSubsidyPercentage { get; set; }
        public decimal SellerSubsidyPercentage { get; set; }
        public decimal MaximumBenefit { get; set; }
        public DateTimeOffset StartsAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset EndsAt { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
    }
}
