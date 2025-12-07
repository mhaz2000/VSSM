using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Arrival
{
    public class ArrivalViewModel
    {
        public int? ArrivalId { get; set; }

        [Display(Name = "تاریخ ورود")]
        [Required(ErrorMessage = "تاریخ ورود الزامی است.")]
        public string DateString { get; set; }   // رشته؛ از datepicker

        [Display(Name = "شماره گزارش")]
        [Required(ErrorMessage = "شماره گزارش الزامی است.")]
        public string ReportNumber { get; set; }

        [Display(Name = "Bidding Number")]
        [Required(ErrorMessage = "Bidding Number الزامی است.")]
        public string BiddingNumber { get; set; }

        [Display(Name = "منبع")]
        [Required(ErrorMessage = "منبع را انتخاب کنید.")]
        public int? SourceId { get; set; }

        [Display(Name = "دوره فعالیت")]
        [Required(ErrorMessage = "دوره فعالیت را انتخاب کنید.")]
        public int? FiscalPeriodId { get; set; }

        [Display(Name = "حجم بسته (لیتر)")]
        public float? PackedVolume { get; set; }

        [Display(Name = "وزن کل (کیلوگرم)")]
        public float? TotalWeight { get; set; }

        [Display(Name = "حجم کل (لیتر)")]
        public float? TotalVolume { get; set; }

        public string Note { get; set; }

        public bool IsConfirmed { get; set; }
        public bool IsDrafted { get; set; }
        public bool IsCancelled { get; set; }

        // برای سوییچ تقویم
        public bool UsePersianDate { get; set; }

        public List<ArrivalLineViewModel> Lines { get; set; } = new();
    }
}
