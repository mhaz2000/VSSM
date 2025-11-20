namespace VSSM.Application.DTOs.BaseEntities.Store
{
    public class StoreListDto
    {
        public int StoreId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public float FreeVolume { get; set; }
        public bool IsActive { get; set; }
    }
}
