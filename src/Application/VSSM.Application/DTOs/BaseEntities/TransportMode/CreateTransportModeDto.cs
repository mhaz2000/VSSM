namespace VSSM.Application.DTOs.BaseEntities.TransportMode;

public class CreateTransportModeDto
{
    public string Code { get; set; }
    public string FullName { get; set; }
    public DateTime? EntryDate { get; set; }
    public bool IsActive { get; set; }
}
