namespace VSSM.Application.DTOs.BaseEntities.FiscalPeriod;

public class FiscalPeriodDto
{
    public int FiscalPeriodId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndTime { get; set; }
    public string FullName { get; set; }
    public string SuggestedEndDate { get; set; }
    public DateTime? EntryDate { get; set; }
    public bool IsActive { get; set; } = true;
}