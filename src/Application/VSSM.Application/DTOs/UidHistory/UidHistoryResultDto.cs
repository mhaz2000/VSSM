namespace VSSM.Application.DTOs.UidHistory;

public class UidHistoryResultDto
{
    public string SearchedUid { get; set; } = default!;
    public List<UidHistoryRowDto> Rows { get; set; } = new();
}
