namespace VSSM.Presentation.Web.Models.Base;

public class GridAction
{
    public string Title { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }

    public string RouteIdProperty { get; set; }

    public string LinkClass { get; set; } = "btn-primary";
    public string IconClass { get; set; } = "fa fa-eye";
}