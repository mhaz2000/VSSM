
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record ItemIndicatorDetailDto
    {
        public string ItemName { get; internal set; }
        public long? CurrentStock { get;  set; }
        public string Manufacturer { get; internal set; }
        public int NumDosePerVial { get; internal set; }
        public DateTime? ExpiryDate { get; internal set; }
        public string ExpiryDateText => ExpiryDate is not null ? ExpiryDate.Value.ToShortDateString() : string.Empty;

        public string BatchNumber { get; internal set; }
    }
}
