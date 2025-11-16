using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.PowerSource
{
    public class PowerSourceViewModel
    {
        public int? PowerSourceId { get; set; }

        [Required(ErrorMessage = "کد نمی‌تواند خالی باشد.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "نام نمی‌تواند خالی باشد.")]
        public string FullName { get; set; }

        public bool IsActive { get; set; }
    }
}
