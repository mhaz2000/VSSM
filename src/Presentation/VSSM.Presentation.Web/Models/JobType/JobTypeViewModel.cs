using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.JobType
{
    public class JobTypeViewModel
    {
        public int? JobTypeId { get; set; }

        [Required(ErrorMessage = "نام نمی‌تواند خالی باشد.")]
        public string FullName { get; set; }

        public bool IsActive { get; set; }
    }
}
