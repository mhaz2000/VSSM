using Microsoft.AspNetCore.Mvc.Rendering;

namespace VSSM.Presentation.Web.Models.ManItemClass;

public class ManItemClassViewModel
{
    public long? ManItemClassId { get; set; }
    public int? ItemClassCode { get; set; }
    public List<int> ManufacturerIds { get; set; } = new();
    public List<SelectListItem> ItemClasses { get; set; } = new();
}


