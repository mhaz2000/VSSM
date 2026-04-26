
using System.Collections.Generic;
using VSSM.Application.DTOs.BaseEntities.Reports;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IReportService
    {
        Task<(List<InventoryQuantityReportDto> data, int count)> GetInventoryQuantitiesAsync(long? itemClassId, long? itemId, long? sourceId, long? storeId, int pageNumber, int pageSize, string sort, string direction, int recipientId, CancellationToken cancellationToken);
        Task<(List<ItemForRecipientInYearDto> data, int count)> GetItemForRecipientInYear(int currentRecipientId, long? itemId, int? year, int? level, int? recipentId, int pageNumber, int pageSize, string sort, string direction, CancellationToken cancellationToken);
        Task<(List<ItemIndicatorDto> data, int count)> GetItemIndicator(int currentRecipientId, int? itemId, string batchNumber, int pageNumber, int pageSize, string sort, string direction, CancellationToken cancellationToken);
        Task<ItemIndicatorDetailDto> GetItemIndicatorDetail(int currentRecipientId,int? itemId, string? batchNumber);
        Task<(List<NotDispatchedToRecipientsDto> data, int count)> GetNotDispatchedToRecipientsAsync(long? itemId, long? fiscalPeriodId, string? fromDate, string? toDate, int pageNumber, int pageSize, string sort, string direction, CancellationToken cancellationToken);
        Task<(List<OutOfStockItemsDto>, int count)> GetOutOfStockItems(int currentRecipientId, int? itemClassId, int pageNumber, int pageSize, string sort, string direction, CancellationToken cancellationToken);
    }
}
