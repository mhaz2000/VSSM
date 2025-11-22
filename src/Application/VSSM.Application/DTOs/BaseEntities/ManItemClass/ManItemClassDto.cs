namespace VSSM.Application.DTOs.BaseEntities.ManItemClass;

public class ManItemClassDto
{
    public long? ManItemClassId { get; set; }
    public int? ItemClassCode { get; set; }

    // Accept multiple Manufacturers from UI
    public List<int> ManufacturerIds { get; set; } = new();

    // Store one in database
    public int? ManufacturerId => 
        ManufacturerIds != null && ManufacturerIds.Count > 0
            ? ManufacturerIds.First()
            : null;
}