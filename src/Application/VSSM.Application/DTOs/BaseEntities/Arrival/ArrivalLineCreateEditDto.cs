using System;
using System.Collections.Generic;

namespace VSSM.Application.DTOs.BaseEntities.Arrival
{
    public class ArrivalCreateDto
    {
        public DateTime Date { get; set; }
        public string ReportNumber { get; set; }
        public int FiscalPeriodId { get; set; }
        public string EnterById { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsDrafted { get; set; }
        public bool IsCancelled { get; set; }

        public string Note { get; set; }


        public List<ArrivalLineDto> Lines { get; set; } = new();
        public int UserRecipientId { get; set; }
    }

    public class ArrivalEditDto : ArrivalCreateDto
    {
        public int ArrivalId { get; set; }
    }
}
