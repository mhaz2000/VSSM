namespace VSSM.Application.DTOs.BaseEntities.VoucherConfig;

public class VoucherConfigDto
{
    public bool ShowExpiryDate { get; init; } = false;
    public bool ShowNumberOfVials { get; init; } = false;
    public bool ShowDoseVial { get; init; } = false;
    public bool ShowVvmtype { get; init; } = false;
    public bool ShowVvmstage { get; init; } = false;
    public bool ShowStore { get; init; } = false;
    public bool ShowUnitPrice { get; init; } = false;
    public bool ShowTotalPriceLine { get; init; } = false;
    public bool ShowNotesLine { get; init; } = false;
    public bool ShowNotes { get; init; } = false;
    public bool ShowTotalWeight { get; init; } = false;
    public bool ShowPacksNumber { get; init; } = false;
    public bool ShowTotalPrice { get; init; } = false;
    public bool ShowTotalExternalSize { get; init; } = false;
    public bool ShowTotalVolume { get; init; } = false;
    public string FooterNotes { get; init; }
    public string SignPlaceHolder1 { get; init; }
    public string SignPlaceHolder2 { get; init; }
    public string SignPlaceHolder3 { get; init; }
    public string SignPlaceHolder4 { get; init; }
    public bool ShowItem { get; init; } = false;
    public bool ShowBatchNumber { get; init; } = false;
    public bool ShowRequestNumber { get; init; } = false;
    public bool ShowTransportMode { get; init; } = false;
    public bool ShowManufacturer { get; init; } = false;
    public bool ShowQuantity { get; init; } = false;
    public bool ShowItemCode { get; init; } = false;
    public bool ShowPresentation { get; init; } = false;
    public bool ShowPackingType { get; init; } = false;
    public bool ShowPackageSize { get; init; } = false;
    public bool ShowStageDelivery { get; init; } = false;
    public bool ShowIndicator { get; init; } = false;
    public bool ShowSource { get; init; } = false;
}
