namespace VSSM.Application.DTOs.BaseEntities.Store;

public class StoreListDto
{
    public int StoreId { get; set; }
    public int? RecipientId { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;

    public float? Volume { get; set; }
    public float? FreeVolume { get; set; }

    public string Note { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public int? Year { get; set; }
}
