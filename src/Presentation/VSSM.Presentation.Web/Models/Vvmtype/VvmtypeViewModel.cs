namespace VSSM.Presentation.Web.Models.Vvmtype;

public class VvmtypeViewModel
{
    public int? VvmtypeId { get; set; }

    public string Code { get; set; }

    public string FullName { get; set; }

    public bool CanUserChange { get; set; }

    public DateTime? EntryDate { get; set; }

    public bool IsActive { get; set; } = true;
}