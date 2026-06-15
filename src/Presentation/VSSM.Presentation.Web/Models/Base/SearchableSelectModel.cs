namespace VSSM.Presentation.Web.Models.Base;

public class SearchableSelectModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }

    public string SelectedValue { get; set; }

    public bool HasDefaultOption { get; set; } = true;
    public string DefaultText { get; set; } = "انتخاب کنید";
    public bool IsRequired { get; set; }
    public string ValidationMessage { get; set; }

    public List<SelectOption> Options { get; set; } = new();
}

public class SelectOption
{
    public string Value { get; set; }
    public string Text { get; set; }
}