using Marketplace.Web.Contracts;

namespace Marketplace.Web.Clients;

public interface IMarketplaceAdminBffClient
{
    Task<AdminProductResponse> CreateProductAsync(CreateAdminProductRequest request, CancellationToken cancellationToken);

    Task<AdminProductResponse?> GetProductAsync(Guid skuId, CancellationToken cancellationToken);

    Task<AdminProductResponse> UpdateProductLogisticsAsync(Guid skuId, UpdateAdminProductLogisticsRequest request, CancellationToken cancellationToken);

    Task<AdminProductResponse> ChangeProductStatusAsync(Guid skuId, ChangeAdminProductStatusRequest request, CancellationToken cancellationToken);
}
