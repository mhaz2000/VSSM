namespace VSSM.Application.DTOs.BaseEntities.JobType
{
    public class JobTypeGetDto
    {
        public int JobTypeId { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
    }
}
