namespace VSSM.Presentation.Web.Models.Manufacturer;

public class ManufacturerViewModel
{
    public int? ManufacturerId { get; set; }
    public string? Code { get; set; }
    public string? FullName { get; set; }
    public DateTime? EntryDate { get; set; }
    public bool ShowForVac { get; set; }
    public bool ShowForOther { get; set; }
    public bool IsActive { get; set; }
}