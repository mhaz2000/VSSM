// Application/DTOs/BaseEntities/MinMaxItem/MinMaxItemListDto.cs
namespace VSSM.Application.DTOs.BaseEntities.MinMaxItem
{
    public class MinMaxItemListDto
    {
        public int MinMaxItemId { get; set; }
        public int? ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public int? TypeOfRoadId { get; set; }
        public string? EntryDate { get; set; }
    }
}
