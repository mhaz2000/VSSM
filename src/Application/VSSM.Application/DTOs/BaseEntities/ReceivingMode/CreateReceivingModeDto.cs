namespace VSSM.Application.DTOs.BaseEntities.ReceivingMode;

public class CreateReceivingModeDto
{
    public string Code { get; set; }
    public string FullName { get; set; }
    //public DateTime? EntryDate { get; set; }
    public bool IsActive { get; set; }
}
