using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.TransportMode;

public class TransportModeDto
{
    public int TransportModeId { get; set; }
    public string Code { get; set; }
    public string FullName { get; set; }
    public DateTime? EntryDate { get; set; }
    public string EntryShamsiDate => EntryDate.HasValue ? EntryDate.Value.ToShamsi() : string.Empty;
    public bool IsActive { get; set; }
}
