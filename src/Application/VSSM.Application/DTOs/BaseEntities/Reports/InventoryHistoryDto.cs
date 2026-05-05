namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record InventoryHistoryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int InStock { get; set; }
    }
}
