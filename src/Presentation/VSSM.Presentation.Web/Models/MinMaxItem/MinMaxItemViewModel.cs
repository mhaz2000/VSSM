// Presentation/Web/Models/MinMaxItem/MinMaxItemViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.MinMaxItem
{
    public class MinMaxItemViewModel
    {
        public int? MinMaxItemId { get; set; }

        [Display(Name = "کالا")]
        [Required(ErrorMessage = "کالا را انتخاب کنید.")]
        public int? ItemId { get; set; }

        [Display(Name = "گیرنده")]
        public int? RecipientId { get; set; }

        [Display(Name = "حداقل موجودی")]
        public long? StockMin { get; set; }

        [Display(Name = "حداکثر موجودی")]
        public long? StockMax { get; set; }

        [Display(Name = "حداقل (قسمت آبی)")]
        public long? Min { get; set; }

        [Display(Name = "حداکثر (قسمت آبی)")]
        public long? Max { get; set; }

        [Display(Name = "نیاز سالانه")]
        public long? AnnualNeeds { get; set; }

 

        [Display(Name = "تعداد ورودها در سال")]
        public long? NumberShipment { get; set; }

        [Display(Name = "سلامت موجودی (%)")]
        public int? PercentSafety { get; set; }

        [Display(Name = "زمان بین درخواست و ورود محموله")]
        public long? LeadConsumption { get; set; }

        [Display(Name = "جمعیت کل")]
        public long? TotalPop { get; set; }

        [Display(Name = "درصد زیر یک سال")]
        public int? PercentUnder { get; set; }

        [Display(Name = "تعداد دوز")]
        public long? NumDose { get; set; }

        [Display(Name = "فاکتور پرت")]
        public long? Wastage { get; set; }

        [Display(Name = "درصد پوشش")]
        public long? PercentCoverage { get; set; }

        [Display(Name = "سطح درخواست")]
        public long? OrderLevel { get; set; }
    }
}
