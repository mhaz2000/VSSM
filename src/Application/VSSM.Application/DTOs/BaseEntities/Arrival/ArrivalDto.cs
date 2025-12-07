using System.Collections.Generic;

namespace VSSM.Application.DTOs.Arrivals
{
    public class ArrivalDto
    {
        public int ArrivalId { get; set; }

        public string DateString { get; set; }
        public string ReportNumber { get; set; }
        public string BiddingNumber { get; set; }

        public int? SourceId { get; set; }
        public string SourceName { get; set; }

        public int? FiscalPeriodId { get; set; }
        public string FiscalPeriodName { get; set; }

        public float? PackedVolume { get; set; }
        public float? TotalWeight { get; set; }
        public float? TotalVolume { get; set; }

        public string Note { get; set; }

        public bool? IsConfirmed { get; set; }
        public bool? IsDrafted { get; set; }
        public bool? IsCancelled { get; set; }

        public List<ArrivalLineDto> Lines { get; set; } = new();
    }
}
