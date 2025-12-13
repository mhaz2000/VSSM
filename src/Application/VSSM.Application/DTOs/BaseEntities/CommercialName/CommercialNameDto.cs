using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.CommercialName;

public class CommercialNameDto
{
    public int CommercialNameId { get; set; }

    public string FullName { get; set; }

    public string Code { get; set; }

    public bool IsActive { get; set; }

    public DateTime? EntryDate { get; set; } = DateTime.Now;
    public string EntryShamsiDate => EntryDate.HasValue ? EntryDate.Value.ToShamsi() : string.Empty;
}