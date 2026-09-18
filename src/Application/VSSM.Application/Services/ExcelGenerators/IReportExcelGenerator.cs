using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.Reports;

namespace VSSM.Application.Services.ExcelGenerators
{
    public interface IReportExcelGenerator
    {
        MemoryStream AvailableQuantityDetailReportExcel(List<GroupedAvailableQuantityDetailDto> data);
        MemoryStream AvailableQuantityQuantitySummaryReportExcel(List<AvailableQuantityQuantitySummaryDto> data);
        MemoryStream AvailableQuantitySummaryReportExcel(List<AvailableQuantitySummaryDto> data);
        MemoryStream AvailableQuantitySummaryWithDosePerVialReportExcel(List<AvailableQuantitySummaryWithDosePerVialDto> data);
        MemoryStream ExpiredItemsReportExcel(List<GroupedExpiredItemDto> data);
        MemoryStream GetTotalLossExcel(List<TotalLossReportDto> data);
        MemoryStream InventoryHistoryExcel(List<InventoryHistoryDto> data);
        MemoryStream ItemForRecipientInYearExcel(List<ItemForRecipientInYearDto> data);
        MemoryStream ItemIndicatorExcel(List<ItemIndicatorDto> data);
        MemoryStream NotDispatchedToRecipientsExcel(List<NotDispatchedToRecipientsDto> data);
        MemoryStream OutOfStockItemsExcel(List<OutOfStockItemsDto> data);
        MemoryStream PhysicalInventoryExcel(List<InventoryQuantityReportDto> data);


        MemoryStream ArrivalByItemReportExcel(List<ArrivalReportDto> data);
        MemoryStream DispatchByRecipientReportExcel(List<DispatchReportDto> data);
        MemoryStream ArrivalSummaryReportExcel(List<ArrivalSummaryDto> data);
    }
}
