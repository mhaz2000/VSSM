using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.EduType
{
    public class EduTypeViewModel
    {
        public int? EduTypeId { get; set; }

        [Required(ErrorMessage = "نام نمی‌تواند خالی باشد.")]
        public string FullName { get; set; }

        public bool IsActive { get; set; }
    }
}
