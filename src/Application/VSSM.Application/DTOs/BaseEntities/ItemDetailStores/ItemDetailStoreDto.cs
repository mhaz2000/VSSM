using System.Text;
using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.BaseEntities.ItemDetailStores;

public class ItemDetailStoreDto
{
    public int ItemDetailStoreId { get; set; }

    public int? ItemDetailId { get; set; }

    public int? StoreId { get; set; }

    public long CurrentStock { get; set; } = 0;

    public DateTime EntryDate { get; set; } = DateTime.Now;
    public string DropDownText { get; set; } = "";
    public int? ItemId { get; set; }
    public string BatchNumber { get; set; }
}