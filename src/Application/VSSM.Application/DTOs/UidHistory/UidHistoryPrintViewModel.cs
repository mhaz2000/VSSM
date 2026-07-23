namespace VSSM.Application.DTOs.UidHistory;

public class UidHistoryPrintViewModel
{
    public string Uid { get; set; } = string.Empty;

    public DateTime PrintDate { get; set; }

    public IReadOnlyList<UidHistoryRowDto> Rows { get; set; }
        = new List<UidHistoryRowDto>();

    public int RowCount => Rows.Count;
}
