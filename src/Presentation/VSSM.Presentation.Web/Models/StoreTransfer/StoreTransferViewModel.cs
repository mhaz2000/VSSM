using Microsoft.AspNetCore.Mvc.Rendering;

namespace VSSM.Presentation.Web.Models.StoreTransfer;

public class StoreTransferViewModel
{
    public int FromStoreId { get; set; }
    public int ToStoreId { get; set; }

    public List<TransferItemInput> Items { get; set; } = new();
    public List<SelectListItem> Stores { get; set; } = new();
}
