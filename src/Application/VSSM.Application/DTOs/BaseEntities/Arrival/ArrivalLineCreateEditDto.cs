using System;
using System.Collections.Generic;

namespace VSSM.Application.DTOs.Arrivals
{
    public class ArrivalLineCreateEditDto
    {
        public int? ArrivalLineId { get; set; } // null برای ردیف جدید

        public int? ItemId { get; set; }
        public int? ManufacturerId { get; set; }
        public int? StoreId { get; set; }
        public int? DosePerVialId { get; set; }
        public int? NumVials { get; set; }
        public int? Quantity { get; set; }
        public float? UnitVolume { get; set; }
        public float? TotalVolume { get; set; }
        public string BatchNum { get; set; }
        public string Note { get; set; }

        public string ExpiryDateString { get; set; }
        public string ProductionDateString { get; set; }
    }

    public class ArrivalCreateEditDto
    {
        public int? ArrivalId { get; set; }

        public string DateString { get; set; }           // yyyy/MM/dd (میلادی به صورت رشته)
        public string ReportNumber { get; set; }
        public string BiddingNumber { get; set; }

        public int? SourceId { get; set; }
        public int? FiscalPeriodId { get; set; }

        public float? PackedVolume { get; set; }
        public float? TotalWeight { get; set; }
        public float? TotalVolume { get; set; }

        public string Note { get; set; }

        public bool SaveAsDraft { get; set; }
        public bool Confirm { get; set; }

        public List<ArrivalLineCreateEditDto> Lines { get; set; } = new();
    }
}
