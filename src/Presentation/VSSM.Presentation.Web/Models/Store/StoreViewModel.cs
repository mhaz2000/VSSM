using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Store
{
    public class StoreViewModel
    {
        public int? StoreId { get; set; }

        [Required(ErrorMessage = "کد نمی‌تواند خالی باشد.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "نام انبار نمی‌تواند خالی باشد.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "نوع درجه حرارت نمی‌تواند خالی باشد.")]
        public int? TypeId { get; set; }

        [Required(ErrorMessage = "حجم نمی‌تواند خالی باشد.")]
        public float? FreeVolume { get; set; }

        public float? Volume { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }

        public int? RecipientId { get; set; }
        public int? Year { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Pqsno { get; set; }
        public int? CoolingUnitNo { get; set; }
        public string RefrigerantGas { get; set; }
 

        public int? PoweSource { get; set; }
        public int? Source { get; set; }

        public string Barcode { get; set; }
        public int? PeriodNotUse { get; set; }
        public float? UnitCost { get; set; }
    }
}
