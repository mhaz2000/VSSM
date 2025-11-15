using VSSM.Application.DTOs.BaseEntities.RoadType;

public class RoadTypeManagementViewModel
{
    public string? Search { get; set; }
    public List<RoadTypeGetDto>? Items { get; set; }

    public RoadTypeEditDto? EditItem { get; set; }
}
