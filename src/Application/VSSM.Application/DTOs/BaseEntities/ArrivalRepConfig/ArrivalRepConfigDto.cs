namespace VSSM.Application.DTOs.BaseEntities.ArrivalRepConfig;

public class ArrivalRepConfigDto
{
    public bool ReportNumber { get; set; }= false;
    public bool Source { get; set; }= false;
    public bool OrderPurchaseNumber { get; set; }= false;
    public bool ItemClass { get; set; }= false;
    public bool ItemCode { get; set; }= false;
    public bool ItemName { get; set; }= false;
    public bool Manufacturer { get; set; }= false;
    public bool BatchNumber { get; set; }= false;
    public bool DosePerVial { get; set; }= false;
    public bool NumberOfVial { get; set; }= false;
    public bool Quantity { get; set; }= false;
    public bool UnitPrice { get; set; }= false;
    public bool TotalPrice { get; set; }= false;
    public bool Activity { get; set; }= false;
    public bool ExpireDate { get; set; }= false;
    public bool UnitVolume { get; set; }= false;
    public bool TotalVolume { get; set; }= false;
    public bool ArrivalDate { get; set; }= false;
}