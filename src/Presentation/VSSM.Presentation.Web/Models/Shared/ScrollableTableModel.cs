namespace VSSM.Presentation.Web.Models.Shared;

public class ScrollableTableModel
{
    public IEnumerable<object> Items { get; set; } = [];
    public List<TableColumn> Columns { get; set; } = [];
    public int VisibleCount { get; set; } = 5;
    public bool ShowActions { get; set; } = true;
    public string EmptyMessage { get; set; } = "آیتمی ثبت نشده است";
    public Func<object, object> ActionTemplate { get; set; }
}

public class TableColumn
{
    public string Header { get; set; } = "";
    public Func<object, int, object> ValueGetter { get; set; }
}