using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Arrival
{
    public class ArrivalViewModel
    {
        public int? ArrivalId { get; set; }

        [Display(Name = "تاریخ ورود")]
        [Required(ErrorMessage = "تاریخ ورود اجباری است.")]
        public string? Date { get; set; } // string – بعداً به DateTime تبدیل می‌کنیم

        [Display(Name = "شماره گزارش")]
        [Required(ErrorMessage = "شماره گزارش اجباری است.")]
        public string ReportNumber { get; set; }

        [Display(Name = "منبع")]
        [Required(ErrorMessage = "منبع را انتخاب کنید.")]
        public int? SourceId { get; set; }

        [Display(Name = "دوره فعالیت")]
        [Required(ErrorMessage = "دوره فعالیت را انتخاب کنید.")]
        public int? FiscalPeriodId { get; set; }

        // فقط نمایش
        public DateTime? EntryDate { get; set; }

        public bool IsConfirmed { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsDrafted { get; set; }

        [Display(Name = "حجم بسته")]
        [Required(ErrorMessage = "حجم بسته را انتخاب کنید.")]
        public float? PackedVolume { get; set; }

        [Display(Name = "وزن کل")]
        [Required(ErrorMessage = "وزن کل را انتخاب کنید.")]
        public float? TotalWeight { get; set; }

        [Display(Name = "حجم کل (لیتر)")]
        [Required(ErrorMessage = "حجم کل (لیتر) را انتخاب کنید.")]
        public float? TotalVolume { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Note { get; set; }

        [Display(Name = "تشخیص دهنده سرما")]
        public int? Frzindc { get; set; }

        [Display(Name = "تشخیص دهنده سرما غیر معتبر")]
        public int? FrzindcNok { get; set; }

        public List<ArrivalLineViewModel> Lines { get; set; } = new();
        public ArrivalLineViewModel NewLine { get; set; } = new();
    }

}
