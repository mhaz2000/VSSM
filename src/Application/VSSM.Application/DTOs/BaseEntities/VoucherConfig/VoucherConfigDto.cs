namespace VSSM.Application.DTOs.BaseEntities.VoucherConfig;

public class VoucherConfigDto
{
    public bool ShowExpiryDate { get; set; }
    public bool ShowNumberOfVials { get; set; }
    public bool ShowDoseVial { get; set; }
    public bool ShowVvmtype { get; set; }
    public bool ShowVvmstage { get; set; }
    public bool ShowStore { get; set; }
    public bool ShowUnitPrice { get; set; }
    public bool ShowTotalPriceLine { get; set; }
    public bool ShowNotesLine { get; set; }
    public bool ShowNotes { get; set; }
    public bool ShowTotalWeight { get; set; }
    public bool ShowPacksNumber { get; set; }
    public bool ShowTotalPrice { get; set; }
    public bool ShowTotalExternalSize { get; set; }
    public bool ShowTotalVolume { get; set; }

    public string? FooterNotes { get; set; }
    public string? SignPlaceHolder1 { get; set; }
    public string? SignPlaceHolder2 { get; set; }
    public string? SignPlaceHolder3 { get; set; }
    public string? SignPlaceHolder4 { get; set; }

    public bool ShowItem { get; set; }
    public bool ShowBatchNumber { get; set; }
    public bool ShowRequestNumber { get; set; }
    public bool ShowTransportMode { get; set; }
    public bool ShowManufacturer { get; set; }
    public bool ShowQuantity { get; set; }
    public bool ShowItemCode { get; set; }
    public bool ShowPresentation { get; set; }
    public bool ShowPackingType { get; set; }
    public bool ShowPackageSize { get; set; }
    public bool ShowStageDelivery { get; set; }
    public bool ShowIndicator { get; set; }
    public bool ShowSource { get; set; }
}
