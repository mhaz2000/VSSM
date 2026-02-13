namespace VSSM.Presentation.Web.Models.Base;

public class PaginationModel
{
    public PagedResult<object> Data { get; set; }
    public string Sort { get; set; }
    public string Direction { get; set; }
    public string Search { get; set; }
}
