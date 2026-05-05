using VSSM.Application.DTOs.BaseEntities.Reports;

namespace VSSM.Application.Services.ExcelGenerators
{
    public interface ICurrentStockReportExcelGenerator
    {
        MemoryStream AvailableQuantityDetailReportExcel(List<GroupedAvailableQuantityDetailDto> data);
        MemoryStream AvailableQuantityQuantitySummaryReportExcel(List<AvailableQuantityQuantitySummaryDto> data);
        MemoryStream AvailableQuantitySummaryReportExcel(List<AvailableQuantitySummaryDto> data);
        MemoryStream AvailableQuantitySummaryWithDosePerVialReportExcel(List<AvailableQuantitySummaryWithDosePerVialDto> data);
        MemoryStream InventoryHistoryExcel(List<InventoryHistoryDto> data);
        MemoryStream ItemForRecipientInYearExcel(List<ItemForRecipientInYearDto> data);
        MemoryStream ItemIndicatorExcel(List<ItemIndicatorDto> data);
        MemoryStream NotDispatchedToRecipientsExcel(List<NotDispatchedToRecipientsDto> data);
        MemoryStream OutOfStockItemsExcel(List<OutOfStockItemsDto> data);
        MemoryStream PhysicalInventoryExcel(List<InventoryQuantityReportDto> data);
    }
}
