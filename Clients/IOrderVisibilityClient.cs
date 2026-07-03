using Marketplace.Web.Contracts;

namespace Marketplace.Web.Clients;

public interface IOrderVisibilityClient
{
    Task<OrderJourneyPagedResult> ListJourneysAsync(OrderJourneyListFilter filter, int page, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderJourneySummary>> ListStuckJourneysAsync(int olderThanSeconds, CancellationToken cancellationToken);

    Task<OrderJourneySummary?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderJourneyEventDetail>?> GetEventsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
}
