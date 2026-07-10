namespace VSSM.Application.DTOs.BaseEntities.Dispatches.Scan;

public class DispatchScanSession
{
    public string SessionId { get; set; }

    public int DispatchId { get; set; }

    public List<ScannedPackage> Items { get; set; } = new();

    public HashSet<string> RemovedUids { get; set; } = new();
}
