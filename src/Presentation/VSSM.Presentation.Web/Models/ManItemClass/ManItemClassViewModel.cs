using Microsoft.AspNetCore.Mvc.Rendering;

namespace VSSM.Presentation.Web.Models.ManItemClass;

public class ManItemClassViewModel
{
    public long? ManItemClassId { get; set; }
    public int? ItemClassCode { get; set; }
    public int? ManufacturerId { get; set; }
    public List<SelectListItem> Manufacturers { get; set; } = new();
}


