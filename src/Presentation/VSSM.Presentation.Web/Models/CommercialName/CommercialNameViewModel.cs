namespace VSSM.Presentation.Web.Models.CommercialName;

public class CommercialNameViewModel
{
    public int? CommercialNameId { get; set; } // Nullable for Create vs Update
    public string FullName { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EntryDate { get; set; }
}