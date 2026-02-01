using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public IActionResult GetStoreCapacity(int storeId)
    {
        //var store = _storeRepository.GetById(storeId);
//
        //if (store == null)
        //    return NotFound();

        return Json(new
        {
            capacity = 1000,
            freeCapacity = 500
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetItemsByStore(int storeId)
    {
        var items = await _itemDetailStoreService.GetActivesByStoreId(storeId,CancellationToken.None);
        
        var x = items.Where(x => x.CurrentStock > 0)
            .Select(x => new
            {
                itemDetailStoreId = x.ItemDetailStoreId,
                itemName = x.ItemDetailId,
                currentStock = x.CurrentStock,
                Bac
            })
            .ToList();

        return Json(x);
    }


    // نمایش فرم انتقال
    [HttpGet]
    public async Task<IActionResult> Transfer()
    {
        var stores = await _storeService.GetAll(CancellationToken.None);

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
    public IActionResult Transfer(StoreTransferViewModel model)
    {
        var selectedItems = model.Items
            .Where(x => x.IsSelected && x.Quantity > 0)
            .ToList();
        
        foreach (var item in selectedItems)
        {
            _itemDetailStoreService.TransferItem(
                model.FromStoreId,
                model.ToStoreId,
                item.ItemDetailStoreId,
                item.Quantity
            );
        }

        return RedirectToAction("Index");
    }
}