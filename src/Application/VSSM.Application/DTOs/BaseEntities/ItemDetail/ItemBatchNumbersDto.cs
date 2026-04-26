
namespace VSSM.Application.DTOs.BaseEntities.ItemDetail;

public record ItemBatchNumbersDto
{
    public string BatchNumber { get; set; }
    public long CurrentStock { get; set; }
}
