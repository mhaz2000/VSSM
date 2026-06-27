using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.ItemDoseAssignment
{
    public class ItemDoseAssignmentViewModel
    {
        public int? ItemClassId { get; set; }
        public int? ItemDoseAssignmentId { get; set; }

        [Required(ErrorMessage = "کالا نباید خالی باشد.")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "دوز بر ویال نباید خالی باشد.")]
        public int DoseId { get; set; }

        public float? VialPerDoseCm3 { get; set; }
    }
}
