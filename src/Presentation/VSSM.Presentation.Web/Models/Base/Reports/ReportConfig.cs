namespace VSSM.Presentation.Web.Models.Base.Reports;

public class ReportConfig
{
    public string Title { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Generate";
    public List<FilterField> Filters { get; set; } = new();
    public List<ActionButton> ActionButtons { get; set; } = new();
    public bool AllowExcel { get; set; } = true;
    public bool IsGrouped { get; set; }
    public string ResultPartialView { get; set; } = "_DynamicGrid";
}
