namespace VSSM.Application.DTOs.BaseEntities.Source
{
    public class SourceGetDto
    {
        public int SourceId { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
    }
}
