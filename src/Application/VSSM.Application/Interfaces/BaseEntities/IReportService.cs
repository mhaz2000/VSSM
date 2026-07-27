
using System.Collections.Generic;
using VSSM.Application.DTOs.BaseEntities.Reports;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IReportService
    {
        Task<(List<GroupedAvailableQuantityDetailDto> data, int count)> GetAvailableQuantityDetailReportAsync(AvailableQuantityReportQuery inputModel, int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<AvailableQuantityQuantitySummaryDto> data, int count)> GetAvailableQuantityQuantitySummaryReportAsync(AvailableQuantityReportQuery inputModel, int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<AvailableQuantitySummaryDto> data, int count)> GetAvailableQuantitySummaryReportAsync(AvailableQuantityReportQuery inputModel, int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<AvailableQuantitySummaryWithDosePerVialDto> data, int count)> GetAvailableQuantitySummaryWithDosePerVialReportAsync(AvailableQuantityReportQuery inputModel, int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<GroupedExpiredItemDto> data, int count)> GetExpiredItemAsync(int currentRecipientId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<InventoryHistoryDto> data, int count)> GetInventoryHistory(int currentRecipientId, long? itemId, int? fiscalPeriodId, int? level, int? recipientId, string? fromDate, string? toDate, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<InventoryQuantityReportDto> data, int count)> GetInventoryQuantitiesAsync(long? itemClassId, long? itemId, long? sourceId, long? storeId, int pageNumber, int pageSize, string sort, string direction, int recipientId, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<ItemForRecipientInYearDto> data, int count)> GetItemForRecipientInYear(int currentRecipientId, long? itemId, int? year, int? level, int? recipentId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<ItemIndicatorDto> data, int count)> GetItemIndicator(int currentRecipientId, int? itemId, string batchNumber, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<ItemIndicatorDetailDto> GetItemIndicatorDetail(int currentRecipientId,int? itemId, string? batchNumber);
        Task<(List<NotDispatchedToRecipientsDto> data, int count)> GetNotDispatchedToRecipientsAsync(long? itemId, long? fiscalPeriodId, string? fromDate, string? toDate, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<OutOfStockItemsDto> data, int count)> GetOutOfStockItems(int currentRecipientId, int? itemClassId, int pageNumber, int pageSize, string sort, string direction, bool fetchAll, CancellationToken cancellationToken);
        Task<(List<TotalLossReportDto> data, int count)> GetTotalLossAsync(long? itemId, DateTime fromDateTime, DateTime toDateTime, int pageNumber, int pageSize, string sort, string direction, bool v, CancellationToken cancellationToken);
    }
}
