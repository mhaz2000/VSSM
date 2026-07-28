namespace VSSM.Application.DTOs.Dashboard;

public class DashboardViewModel
{
    public int ArrivedShipments { get; set; }
    public int DraftOutgoing { get; set; }
    public int DraftIncoming { get; set; }
    public List<ChartData> BelowMinItems { get; set; } = [];
    public List<ChartData> BelowRequestItems { get; set; } = [];
    public List<ChartData> NearExpiryItems { get; set; } = [];
    public List<ChartData> AboveMaxItems { get; set; } = [];
}

public class ChartData
{
    public string Label { get; set; }
    public int Value { get; set; }
}
