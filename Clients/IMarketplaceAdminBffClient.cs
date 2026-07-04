using Marketplace.Web.Contracts;

namespace Marketplace.Web.Clients;

public interface IMarketplaceAdminBffClient
{
    Task<AdminProductResponse> CreateProductAsync(CreateAdminProductRequest request, CancellationToken cancellationToken);

    Task<AdminProductResponse?> GetProductAsync(Guid skuId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminProductResponse>> ListProductsAsync(CancellationToken cancellationToken);

    Task<AdminProductResponse> UpdateProductLogisticsAsync(Guid skuId, UpdateAdminProductLogisticsRequest request, CancellationToken cancellationToken);

    Task<AdminProductResponse> ChangeProductStatusAsync(Guid skuId, ChangeAdminProductStatusRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminInventoryItemResponse>> SearchInventoryAsync(
        Guid? sellerId, Guid? skuId, Guid? fulfillmentCenterId, CancellationToken cancellationToken);

    Task AdjustStockAsync(AdminStockAdjustmentRequest request, string idempotencyKey, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminFulfillmentCenterResponse>> ListFulfillmentCentersAsync(CancellationToken cancellationToken);

    Task<AdminFulfillmentCenterResponse?> GetFulfillmentCenterAsync(Guid fulfillmentCenterId, CancellationToken cancellationToken);

    Task<AdminFulfillmentCenterResponse> CreateFulfillmentCenterAsync(CreateAdminFulfillmentCenterRequest request, CancellationToken cancellationToken);

    Task ChangeFulfillmentCenterStatusAsync(Guid fulfillmentCenterId, ChangeAdminFulfillmentCenterStatusRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminCapacitySlotResponse>> GetCapacitySlotsAsync(Guid fulfillmentCenterId, CancellationToken cancellationToken);

    Task ConfigureCapacityAsync(Guid fulfillmentCenterId, AdminConfigureCapacityRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminLogisticsNodeResponse>> ListNodesAsync(string? region, CancellationToken cancellationToken);

    Task<AdminLogisticsNodeResponse> CreateNodeAsync(CreateAdminNodeRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminLogisticsLaneResponse>> ListLanesAsync(CancellationToken cancellationToken);

    Task<AdminLogisticsLaneResponse> CreateLaneAsync(CreateAdminLaneRequest request, CancellationToken cancellationToken);

    Task<AdminLogisticsLaneResponse> ChangeLaneStatusAsync(Guid laneId, ChangeAdminLaneStatusRequest request, CancellationToken cancellationToken);

    Task<AdminLogisticsLaneResponse> UpdateLaneAsync(Guid laneId, UpdateAdminLaneRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminCarrierResponse>> ListCarriersAsync(CancellationToken cancellationToken);

    Task<AdminCarrierResponse> CreateCarrierAsync(CreateAdminCarrierRequest request, CancellationToken cancellationToken);

    Task ChangeCarrierStatusAsync(string carrierCode, ChangeAdminCarrierStatusRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminServiceLevelResponse>> ListServiceLevelsAsync(string carrierCode, CancellationToken cancellationToken);

    Task<AdminServiceLevelResponse> CreateServiceLevelAsync(string carrierCode, CreateAdminServiceLevelRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminCarrierLaneResponse>> ListCarrierLanesAsync(string carrierCode, CancellationToken cancellationToken);

    Task<AdminCarrierLaneResponse> CreateCarrierLaneAsync(string carrierCode, CreateAdminCarrierLaneRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminRateCardResponse>> ListRateCardsAsync(CancellationToken cancellationToken);

    Task<AdminRateCardResponse> CreateRateCardAsync(AdminRateCardRequest request, CancellationToken cancellationToken);

    Task<AdminRateCardResponse> ActivateRateCardAsync(Guid rateCardId, CancellationToken cancellationToken);

    Task<AdminRateCardResponse> RetireRateCardAsync(Guid rateCardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminPromotionRuleResponse>> ListSubsidyRulesAsync(CancellationToken cancellationToken);

    Task<AdminPromotionRuleResponse> CreateSubsidyRuleAsync(CreateAdminPromotionRuleRequest request, CancellationToken cancellationToken);

    Task<AdminPromotionRuleResponse> ChangeSubsidyRuleActiveAsync(Guid promotionId, bool isActive, CancellationToken cancellationToken);

    Task<AdminShippingPromiseSimulationResponse> SimulateShippingPromiseAsync(SimulateAdminShippingPromiseRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminOrderListItemResponse>> ListOrdersAsync(
        Guid? buyerId, Guid? sellerId, string? status, CancellationToken cancellationToken);

    Task<AdminOrderResponse?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken);

    Task CancelOrderAsync(Guid orderId, CancelAdminOrderRequest request, string idempotencyKey, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminShipmentListItemResponse>> ListShipmentsAsync(Guid? orderId, CancellationToken cancellationToken);

    Task<AdminShipmentResponse?> GetShipmentAsync(Guid shipmentId, CancellationToken cancellationToken);

    Task<AdminShipmentLabelResponse?> GetShipmentLabelAsync(Guid shipmentId, CancellationToken cancellationToken);

    Task CancelShipmentAsync(Guid shipmentId, string idempotencyKey, CancellationToken cancellationToken);

    Task<AdminShipmentTrackingResponse?> GetShipmentTrackingAsync(Guid shipmentId, CancellationToken cancellationToken);

    Task<AdminTrackingEventAcceptedResponse> CreateTrackingEventAsync(CreateAdminTrackingEventRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminAuditLogEntryResponse>> ListAuditLogAsync(
        string? entityType, string? action, string? adminUserId, int? limit, CancellationToken cancellationToken);
}
