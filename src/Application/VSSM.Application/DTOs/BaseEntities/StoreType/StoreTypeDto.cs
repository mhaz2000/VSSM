using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.StoreType;

public class StoreTypeDto
{
    public int StoreTypeId { get; set; }
    public string Code { get; set; }
    public string FullName { get; set; }
    public DateTime? EntryDate { get; set; }
    public string EntryShamsiDate => EntryDate.HasValue ? EntryDate.Value.ToShamsi() : string.Empty;

    public bool IsActive { get; set; }
    public bool IsUserEditable { get; set; }
}