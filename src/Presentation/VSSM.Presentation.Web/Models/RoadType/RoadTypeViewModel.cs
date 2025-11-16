using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.RoadType
{
    public class RoadTypeViewModel
    {
        public int? RoadTypeId { get; set; }

        [Required(ErrorMessage = "کد الزامی است")]
        public string Code { get; set; }

        [Required(ErrorMessage = "نام کامل الزامی است")]
        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
