namespace VSSM.Presentation.Web.Models.Base.Reports;

public class ReportResult
{
    public string Title { get; set; } = string.Empty;
    public object GridData { get; set; } = new();
    public bool IsGrouped { get; set; }
    public Dictionary<string, string> AdditionalInfo { get; set; } = new();
    public string ExcelAction { get; set; } = string.Empty;
    public bool AllowExcel { get; set; } = true;
}
