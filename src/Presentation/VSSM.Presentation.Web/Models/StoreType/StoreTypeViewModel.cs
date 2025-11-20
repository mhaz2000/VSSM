namespace VSSM.Presentation.Web.Models.StoreType;

public class StoreTypeViewModel
{
    public int? StoreTypeId { get; set; }
    public string Code { get; set; }
    public string FullName { get; set; }
    public DateTime? EntryDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsUserEditable { get; set; }
}