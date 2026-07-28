namespace VSSM.Presentation.Web.Models.Base.Reports;

public class ReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, string> Filters { get; set; } = new();
    public bool ToExcel { get; set; }
}
