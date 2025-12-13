namespace VSSM.Application.DTOs.BaseEntities.Arrival
{
    public class ArrivalListDto
    {
        public int ArrivalId { get; set; }
        public string ReportNumber { get; set; }
        public DateTime? Date { get; set; }

        public string SourceName { get; set; }
        public string FiscalPeriodName { get; set; }

        public float? TotalVolume { get; set; }

        public bool? IsConfirmed { get; set; }
        public bool? IsDrafted { get; set; }
        public bool? IsCancelled { get; set; }
    }


}



namespace VSSM.Application.DTOs.BaseEntities.Arrival
{
    public class ArrivalGetDto
    {
        public int ArrivalId { get; set; }

        public DateTime? Date { get; set; }
        public string ReportNumber { get; set; }

        public int? SourceId { get; set; }
        public int? FiscalPeriodId { get; set; }

        public DateTime? EntryDate { get; set; }

        public bool? IsConfirmed { get; set; }
        public bool? IsCancelled { get; set; }
        public bool? IsDrafted { get; set; }

        public float? PackedVolume { get; set; }
        public float? TotalWeight { get; set; }
        public float? TotalVolume { get; set; }

        public string Note { get; set; }

        public int? Frzindc { get; set; }
        public int? FrzindcNok { get; set; }

        public List<ArrivalLineDto> Lines { get; set; } = new();
    }
}
