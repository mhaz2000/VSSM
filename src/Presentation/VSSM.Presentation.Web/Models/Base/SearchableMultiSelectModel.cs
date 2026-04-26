namespace VSSM.Presentation.Web.Models.Base
{

    public class SearchableMultiSelectModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }

        public List<string> SelectedValues { get; set; } = new();

        public bool IsRequired { get; set; }

        public string DefaultText { get; set; } = "انتخاب کنید";

        public List<SelectOption> Options { get; set; } = new();
    }
}
