namespace VSSM.Application.DTOs.BaseEntities.Vvmstage;

public class VvmstageDto
{
    public int VvmstageId { get; set; }
    public string Code { get; set; }
    public string FullName { get; set; }
    public DateTime? EntryDate { get; set; }
    public bool IsActive { get; set; }
}