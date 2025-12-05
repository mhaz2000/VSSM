using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.FiscalPeriod;

public class FiscalPeriodDto
{
    public int FiscalPeriodId { get; set; }
    public DateTime? StartDate { get; set; }
    public string? StartDateShamsiDate=> EntryDate.HasValue ? EntryDate.Value.ToShamsi() : string.Empty;
    public DateTime? EndTime { get; set; }
    public string? EndTimeShamsiDate=> EntryDate.HasValue ? EntryDate.Value.ToShamsi() : string.Empty;
    public string FullName { get; set; }
    public string SuggestedEndDate { get; set; }
    public DateTime? EntryDate { get; set; }
    public string EntryShamsiDate => EntryDate.HasValue ? EntryDate.Value.ToShamsi() : string.Empty;
    public bool IsActive { get; set; } = true;
}