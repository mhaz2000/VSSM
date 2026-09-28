namespace VSSM.Application.DTOs.BaseEntities.Reports;

public record ItemForRecipientInYearQuery
{
    public int? RecipientId { get; set; }
    public int? Level { get; set; }
    public long? ItemId { get; set; }
    public int? Year { get; set; }
}