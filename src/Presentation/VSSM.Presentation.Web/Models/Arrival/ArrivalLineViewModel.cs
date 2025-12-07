using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Arrival
{
    public class ArrivalLineViewModel
    {
        public int? ArrivalLineId { get; set; }

        [Display(Name = "کالا")]
        [Required(ErrorMessage = "کالا الزامی است.")]
        public int? ItemId { get; set; }

        [Display(Name = "سازنده")]
        public int? ManufacturerId { get; set; }

        [Display(Name = "انبار")]
        [Required(ErrorMessage = "انبار الزامی است.")]
        public int? StoreId { get; set; }

        [Display(Name = "دوز بر ویال")]
        public int? DosePerVialId { get; set; }

        [Display(Name = "تعداد دوز")]
        [Required(ErrorMessage = "تعداد الزامی است.")]
        public int? Quantity { get; set; }

        [Display(Name = "تعداد ویال")]
        public int? NumVials { get; set; }

        [Display(Name = "حجم واحد (لیتر)")]
        public float? UnitVolume { get; set; }

        [Display(Name = "حجم کل (لیتر)")]
        public float? TotalVolume { get; set; }

        [Display(Name = "سری ساخت")]
        public string BatchNum { get; set; }

        [Display(Name = "تاریخ انقضا")]
        public string ExpiryDateString { get; set; }

        [Display(Name = "تاریخ تولید")]
        public string ProductionDateString { get; set; }

        public string Note { get; set; }
    }
}
