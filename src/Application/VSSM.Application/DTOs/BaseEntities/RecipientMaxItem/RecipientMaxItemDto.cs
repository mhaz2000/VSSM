namespace VSSM.Application.DTOs.BaseEntities.RecipientMaxItem;

public class RecipientMaxItemDto
{
    public int RecipientMaxItemId { get; set; }
    public int? ItemId { get; set; }
    public int? RecipientId { get; set; }
    public long? NumberDose { get; set; }
    public DateTime? EntryDate { get; set; } =  DateTime.Now;
    public int? ReceiverId { get; set; }
    public int? RelatedToItem { get; set; }
}