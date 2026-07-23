namespace VSSM.Application.DTOs.UidHistory;

public class UidHistoryRowDto
{
    public int RowNumber { get; set; }
    public string? EventTitle { get; set; }   
    public string? EventDate { get; set; }        
    public string? Description { get; set; }      
    public int SortOrder { get; set; }            
}
