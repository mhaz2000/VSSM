namespace VSSM.Application.DTOs.BaseEntities.Dispatches.Scan;

public class DispatchScanSession
{
    public string SessionId { get; set; }

    public int DispatchId { get; set; }

    public List<ScannedPackage> Items { get; set; }
        = new();

    public HashSet<string> ScannedUids { get; set; }
       = new();

    public HashSet<int> SelectedChildIds { get; set; }
        = new();

    public Dictionary<string, ScannedPackage> PackageIndex { get; set; }
        = new();

    public HashSet<string> RemovedUids { get; set; }
        = new();

    public int? ItemId { get; set; }

    public string BatchNumber { get; set; }
}
