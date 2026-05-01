
using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record ItemIndicatorDto
    {
        public string RequestNum { get; internal set; }
        public string ReportNumber { get; internal set; }
        public DateTime? Date { get; internal set; }
        public string DateText => Date is not null ? Date.Value.ToShamsi() : string.Empty;
        public string Recipient { get; internal set; }
        public long? CurrentStock { get; internal set; }
        public int? Arrival { get; set; }
        public int? Dispatch { get; set; }
        public string Description { get; set; }
    }
}
