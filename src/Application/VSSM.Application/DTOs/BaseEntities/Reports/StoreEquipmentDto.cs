namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record StoreEquipmentDto 
    {
        public string StoreName { get; set; }
        public string StoreType { get; set; }
        public float? Capacity { get; set; }
        public float? TotalCapacity { get; set; }
        public int? Year { get; set; }
        public string IsActive { get; set; }
    };
}
