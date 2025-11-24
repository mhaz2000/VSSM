namespace VSSM.Application.DTOs.BaseEntities.DispatchRepConfig;

public class DispatchRepConfigDto
{

    public bool DispatchDate { get; set; }           = false;
    public bool Recipient { get; set; } = false;
    public bool RequestNumber { get; set; } = false;
    public bool ItemCode { get; set; } = false;
    public bool ItemClass { get; set; } = false;
    public bool ItemName { get; set; } = false;
    public bool BatchNumber { get; set; } = false;
    public bool VoucherNumber { get; set; } = false;
    public bool Quantity { get; set; } = false;
    public bool DosePerVial { get; set; } = false;
    public bool NumberOfVial { get; set; } = false;
    public bool ExpDate { get; set; } = false;
    public bool Store { get; set; } = false;
    public bool Activity { get; set; } = false;
}