namespace VSSM.Application.DTOs.BaseEntities.Item;

public class ItemListDto
{
    public int ItemId { get; set; }
    public int? FkClassId { get; set; }
    public string Code { get; set; }
    public string FullName { get; set; }
    public int? DiluentId { get; set; }
    public string DiluentName { get; set; }
    public bool IsActive { get; set; }
    public int? OrderInList { get; set; }
    public string UniCode { get; set; }

}
