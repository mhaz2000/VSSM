namespace VSSM.Application.DTOs.BaseEntities.Warnings;

public record CloseToExpirationDto
{
    public string ItemName { get; set; }
    public long Quantity { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public string ExpiredDateString => ExpiredDate is not null ? ExpiredDate.Value.ToShortDateString() : string.Empty;
    public string? DaysToExpired => ExpiredDate is not null ? $"{(ExpiredDate.Value.Date - DateTime.Now.Date).TotalDays} روز" : null;
    public string? Manufacturer { get; set; }
    public string? BatchNumber { get; set; }
    public string? VVMStage { get; set; }
    public string? StockName { get; set; }
}