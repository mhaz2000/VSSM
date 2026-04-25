namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record StoreEquipmentDto 
    {
        public string StoreName { get; set; }
        public string StoreType { get; set; }
        public int Capacity { get; set; }
        public int TotalCapacity { get; set; }
        public int IsActive { get; set; }
    };
}
