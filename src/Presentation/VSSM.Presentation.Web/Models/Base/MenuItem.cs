namespace VSSM.Presentation.Web.Models.Base
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string Icon { get; set; } 
        public string Controller { get; set; }
        public string Action { get; set; }
        public List<MenuItem> Children { get; set; } = new();
    }

}
