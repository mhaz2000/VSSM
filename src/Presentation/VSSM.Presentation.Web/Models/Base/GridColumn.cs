namespace VSSM.Presentation.Web.Models.Base;

public class GridColumn
{
    public string Header { get; set; } = string.Empty;
    public string? PropertyName { get; set; }
    public bool Sortable { get; set; }
    public bool IsActionColumn { get; set; }
    public bool IsAmount { get; set; }
    public bool IsBadgeColumn { get; set; }

    /// نگاشت مقدار → (متن نمایشی، کلاس CSS badge)
    /// مثال:
    ///   ["True"]  = ("فعال",    "badge-success")
    ///   ["False"] = ("غیرفعال", "badge-danger")
    ///   ["1"]     = ("پیش‌نویس","badge-warning")
    ///   ["2"]     = ("تایید",   "badge-success")
    public Dictionary<string, (string Text, string CssClass)>? BadgeMap { get; set; }

}
