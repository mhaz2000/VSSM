namespace VSSM.Presentation.Web.Models.Base.Reports;

public class FilterField
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FilterType Type { get; set; } = FilterType.Select;
    public bool Required { get; set; }
    public List<SelectOption> Options { get; set; } = new();
    public string Placeholder { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string DependsOn { get; set; } = string.Empty;
}

public enum FilterType
{
    Select,
    MultiSelect,
    Date,
    Text,
    Checkbox
}
