namespace VSSM.Presentation.Web.Models.Vvmstage;

public class VvmstageViewModel
{
    public int? VvmstageId { get; set; }

    public string Code { get; set; }

    public string FullName { get; set; }

    public DateTime? EntryDate { get; set; }

    public bool IsActive { get; set; } = true;
}