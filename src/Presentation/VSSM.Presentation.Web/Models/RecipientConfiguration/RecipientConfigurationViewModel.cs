using System;
using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.RecipientConfiguration
{
    public class RecipientConfigurationViewModel
    {
        public int? RecipientConfigurationId { get; set; }

        [Display(Name = "استفاده از بسته‌ها")]
        public bool UseBundles { get; set; }

        [Display(Name = "مقدار حلال در ارسالات قابل تغییر نباشد")]
        public bool FreezeQtyDispatchedDl { get; set; }

        [Display(Name = "زبان جاری")]
        [Required(ErrorMessage = "زبان جاری را انتخاب کنید.")]
        public string CurrentLang { get; set; }   // "fa" or "en"

        [Display(Name = "تعداد حلال جاری")]
        public int? CurrentVoucherNumber { get; set; }

        [Display(Name = "محل خدمت کاربر لاگین")]
        public int? RecipientId { get; set; }

    }
}
