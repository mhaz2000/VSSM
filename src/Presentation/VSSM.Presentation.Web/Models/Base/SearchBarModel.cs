namespace VSSM.Presentation.Web.Models.Base;
public class SearchBarModel
{
    public string SearchTerm { get; set; }
    public int PageSize { get; set; }
    public string Sort { get; set; }
    public string Direction { get; set; }
}