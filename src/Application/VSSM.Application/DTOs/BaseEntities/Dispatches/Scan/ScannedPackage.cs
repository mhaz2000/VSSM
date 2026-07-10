using VSSM.Domain.Enums;

namespace VSSM.Application.DTOs.BaseEntities.Dispatches.Scan;

public class ScannedPackage
{
    public string Uid { get; set; }

    public UidLevel Level { get; set; }

    public string DisplayName { get; set; }

    public int Count { get; set; }

    public int ItemId { get; set; }

    public string BatchNumber { get; set; }


    public bool IsExpanded { get; set; }

    public List<string> ChildUids { get; set; } = new();
}
