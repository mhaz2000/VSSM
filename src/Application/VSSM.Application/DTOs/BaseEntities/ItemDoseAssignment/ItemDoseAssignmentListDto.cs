namespace VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment
{
    public class ItemDoseAssignmentListDto
    {
        public int ItemDoseAssignmentId { get; set; }
        public int? ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int? DoseId { get; set; }
        public int DoseNum { get; set; }
        public float? VialPerDoseCm3 { get; set; }
    }
}
