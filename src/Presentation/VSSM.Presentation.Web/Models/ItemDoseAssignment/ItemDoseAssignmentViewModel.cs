namespace VSSM.Presentation.Web.Models.ItemDoseAssignment
{
    public class ItemDoseAssignmentViewModel
    {
        public int? ItemDoseAssignmentId { get; set; }

        public int ItemId { get; set; }

        public int DoseId { get; set; }

        public float VialPerDoseCm3 { get; set; }
    }
}
