using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.StoreTransfers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.StoreTransfer;

namespace VSSM.Presentation.Web.Controllers;

public class StoreTransferController : BaseController
{
    private readonly IStoreService _storeService;
    private readonly IItemDetailStoreService _itemDetailStoreService;

    public StoreTransferController(IStoreService storeService, IItemDetailStoreService itemDetailStoreService)
    {
        _storeService = storeService;
        _itemDetailStoreService = itemDetailStoreService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStoreCapacity(int storeId)
    {
        var store = await _storeService.GetById(storeId, CancellationToken.None);

        if (store == null)
            return NotFound();

        return Json(new
        {
            capacity = store.Volume,
            freeCapacity = store.FreeVolume
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetItemsByStore(int storeId)
    {
        var items = await _itemDetailStoreService.GetActivesByStoreId(storeId, CancellationToken.None);

        var x = items.Where(x => x.CurrentStock > 0)
            .Select(x => new
            {
                itemDetailStoreId = x.ItemDetailStoreId,
                itemName = x.ItemDetailId,
                currentStock = x.CurrentStock,
            })
            .ToList();

        return Json(x);
    }


    // نمایش فرم انتقال
    [HttpGet]
    public async Task<IActionResult> Transfer()
    {
        var stores = await _storeService.GetAllWithRecipientId((int)CurrentUserRecipientId! ,CancellationToken.None);

        var model = new StoreTransferViewModel
        {
            Stores = stores.Select(x => new SelectListItem
            {
                Value = x.StoreId.ToString(),
                Text = x.FullName
            }).ToList()
        };

        return View(model);
    }

    // ثبت انتقال
    [HttpPost]
    public async Task<IActionResult> Transfer(StoreTransferViewModel model)
    {
        var dto = new StoreTransfersDto()
        {
            ToStoreId = model.ToStoreId,
        };

        dto.Items.AddRange(model.Items.Select(c => new TransferItem()
        {
            Quantity = c.Quantity,
            ItemDetailStoreId = c.ItemDetailStoreId
        }));

        await _itemDetailStoreService.TransferItems(dto);

        TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
        TempData["ToastType"] = "success";
        
        return RedirectToAction("Transfer");
    }
}