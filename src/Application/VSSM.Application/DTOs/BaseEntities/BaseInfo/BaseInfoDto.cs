namespace VSSM.Application.DTOs.BaseEntities.BaseInfo;

public record BaseInfoDto
{
    public int Id { get; internal set; }
    public string? Title { get; internal set; }
    public string? Code { get; internal set; }
    public DateTime? UpdatedDate { get; internal set; }
    public string UpdatedDateText => UpdatedDate?.ToShortDateString() ?? string.Empty;
    public string? ParentTitle { get; internal set; }
    public int? Order { get; internal set; }
    public int? ParentId { get; internal set; }
}
