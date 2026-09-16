using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record GroupedAvailableQuantityDetailDto
{
    public AvailableQuantityDetailMetaDto Meta { get; set; }
    public List<AvailableQuantityDetailDto> Details { get; set; } = new();
}

public record AvailableQuantityDetailMetaDto
{
    [Display(Name = "نام کالا")]
    public string ItemName { get; internal set; }

    [Display(Name = "تعداد")]
    public long Count { get; set; }
}

public record AvailableQuantityDetailDto
{
    public string ItemName { get; internal set; }
    public string BatchNumber { get; internal set; }
    public string ManufacturerName { get; internal set; }
    public DateTime? ExpiryDate { get; internal set; }

    public string ShamsiExpiryDate =>
        ExpiryDate is not null ? ExpiryDate.Value.ToShortDateString() : string.Empty;

    public int DaysToBeExpired =>
        ExpiryDate is not null
            ? (ExpiryDate.Value.Date - DateTime.Now.Date).Days
            : 0;

    public string DoesPerVial { get; internal set; }

    public string Stock { get; internal set; }
    public string Recipient { get; internal set; }
    public DateTime? ProductionDate { get; internal set; }

    public string ProductionDateText =>
        ProductionDate is not null ? ProductionDate.Value.ToShortDateString() : string.Empty;

    public string CommercialName { get; internal set; }
    public string Vvmstage { get; internal set; }
    public string Vvmtype { get; internal set; }
    public long? CurrentStock { get; internal set; }
}