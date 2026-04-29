namespace VSSM.Application.DTOs.BaseEntities.Store
{
    public class StoreListDto
    {
        public int StoreId { get; set; }
        public int? RecipientId { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }

        public int TypeId { get; set; }
        public string TypeName { get; set; }

        public float? Volume { get; set; }
        public float? FreeVolume { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
        public int? Year { get; set; }
    }

}
