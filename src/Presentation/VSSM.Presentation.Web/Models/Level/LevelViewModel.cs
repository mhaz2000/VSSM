namespace VSSM.Presentation.Web.Models.Level;

public class LevelViewModel
{
    public int? LevelId { get; set; }
    public int? ShipCount { get; set; }
    public double? SafetyStockPercentage { get; set; }
    public double? SafetyStockMonth { get; set; }
    public string Note { get; set; }
    public int? MaxDay { get; set; }
}