namespace VSSM.Presentation.Web.Models.Base;

public class GridColumn
{
    public bool IsAmount { get; set; } = false;
    public string Header { get; set; }
    public string PropertyName { get; set; }

    public bool Sortable { get; set; } = false;
}
