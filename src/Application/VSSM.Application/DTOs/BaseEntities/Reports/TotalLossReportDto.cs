namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record TotalLossReportDto
    {
        public string Item { get; internal set; }
        public string DestroyReason { get; set; }
        public string DestroyBy { get; set; }
        public string Date { get; set; }
    }
}
