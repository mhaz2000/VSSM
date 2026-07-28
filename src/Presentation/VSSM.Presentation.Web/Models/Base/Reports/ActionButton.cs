namespace VSSM.Presentation.Web.Models.Base.Reports;

public class ActionButton
{
    public string Text { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string CssClass { get; set; } = "btn-primary";
    public string Icon { get; set; } = "fa fa-search";
}
