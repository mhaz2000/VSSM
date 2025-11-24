using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Pack
{
    public class PackViewModel
    {
        public int? PackId { get; set; }

        [Required(ErrorMessage = "کد نمی‌تواند خالی باشد.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "نام نمی‌تواند خالی باشد.")]
        public string FullName { get; set; }

        public float? NetVolume { get; set; }
        public float? ExternalSize { get; set; }
        public float? PackWeight { get; set; }
        public bool IsActive { get; set; }

        public bool UseForVacDL { get; set; }
        public bool UseForOS { get; set; }
    }

}
