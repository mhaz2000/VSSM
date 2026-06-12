namespace VSSM.Presentation.Web.Models.Base;

public class GridAction
{
    public string Title { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string RouteIdProperty { get; set; } = string.Empty;
    public string LinkClass { get; set; } = "btn-primary";
    public string IconClass { get; set; } = "fa fa-eye";
    public string IconStyle { get; set; } = "";
    public bool OpenModal { get; set; } = false;
    public string Method { get; set; } = "GET";

    public Func<object, string>? DynamicTitle { get; set; }
    public Func<object, string>? DynamicSkip { get; set; }
    public Func<object, string>? DynamicAction { get; set; }
    public Func<object, string>? DynamicController { get; set; }
    public Func<object, string>? DynamicIconClass { get; set; }
    public Func<object, string>? DynamicIconStyle { get; set; }
    public Func<object, string>? DynamicLinkClass { get; set; }
}