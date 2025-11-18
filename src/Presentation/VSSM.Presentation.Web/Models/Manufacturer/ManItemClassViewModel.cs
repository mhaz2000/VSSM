using Microsoft.AspNetCore.Mvc.Rendering;

namespace VSSM.Presentation.Web.Models.Manufacturer;

public class ManItemClassViewModel
{
    public long ManItemClassId { get; set; }

    // --- Data Fields ---
    public int? ItemClassId { get; set; }

    // The Range attribute forces selection of a valid ID > 0
    public int? ManufacturerId { get; set; }

    // --- Display/Lookup Fields ---
    public string ManufacturerFullName { get; set; } // For Index View

    // --- Dropdown List ---
    // This SelectList will be populated by the Controller
    public SelectList Manufacturers { get; set; } = new SelectList(new List<SelectListItem>());
}