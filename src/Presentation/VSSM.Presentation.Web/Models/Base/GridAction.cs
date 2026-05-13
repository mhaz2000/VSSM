namespace VSSM.Presentation.Web.Models.Base;

public class GridAction
{
    public string Title { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }

    public string RouteIdProperty { get; set; }

    public string LinkClass { get; set; } = "btn-primary";
    public string IconClass { get; set; } = "fa fa-eye";
    public string IconStyle { get; set; } = "";

    // Dynamic values
    public Func<object, string>? DynamicTitle { get; set; }

    public Func<object, string>? DynamicAction { get; set; }

    public Func<object, string>? DynamicController { get; set; }

    public Func<object, string>? DynamicIconClass { get; set; }

    public Func<object, string>? DynamicIconStyle { get; set; }

    public Func<object, string>? DynamicLinkClass { get; set; }
}