using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.DestroyUid;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class DestroyUidService(VSSMDbContext db) : IDestroyUidService  // DbContext خودت
{
    public async Task<List<(int Id, string Title)>> GetDestroyReasonsAsync(CancellationToken token)
    {

        return await db.TblBaseInfo
            .AsNoTracking()
            .Where(b => b.ParentId == 37)
            .OrderBy(b => b.Order)
            .Select(b => new ValueTuple<int, string>(b.BaseInfoId, b.Title))
            .ToListAsync(token);
    }

    public async Task<(bool Success, string Message, List<DestroyUidSearchResultDto> Data)>
        SearchByBatchAsync(string batchNumber, int userStoreId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return (false, "سری ساخت اجباری است", []);

        var order = await db.TblOrderInfo
            .AsNoTracking()
            .Include(o => o.TblUidInfos)
            .FirstOrDefaultAsync(o =>
                o.BatchNumber == batchNumber && o.Status == "APPLIED", token);

        if (order == null)
            return (false, "سری ساخت یافت نشد", []);

        var uidCount = order.TblUidInfos.Count(u =>
            u.IsActive == true &&
            u.Used == false &&
            u.FkDestroyId == null &&
            u.CurrentStore == userStoreId);

        if (uidCount == 0)
            return (false, "سری ساخت وارد شده فاقد UID معتبر می‌باشد", []);

        var itemName = await db.TblItems
            .Where(i => i.ItemId == order.FkItemId)
            .Select(i => i.FullName)
            .FirstOrDefaultAsync(token) ?? "-";

        var result = new DestroyUidSearchResultDto
        {
            PkOrderInfo = order.PkOrderInfo,
            BatchNumber = order.BatchNumber,
            ItemName = itemName,
            DosePerVial = order.FkDosePerVialId,
            ProductionDate = order.ManufactureDate.ToShamsi(),
            ExpiryDate = order.ExpirationDate?.ToShamsi() ?? "-",
            UidCount = uidCount
        };

        return (true, string.Empty, [result]);
    }

    public async Task<(bool Success, string Message, List<DestroyUidSearchResultDto> Data)>
        SearchByUidAsync(string uid, int userRecipientId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(uid))
            return (false, "UID اجباری است", []);
        if (uid.Length < 20)
            return (false, "UID وارد شده معتبر نمی‌باشد", []);

        var validStoreIds = await db.TblStores
            .Where(s => s.RecipientId == userRecipientId)
            .Select(s => s.StoreId)
            .ToListAsync(token);

        var uidRecord = await db.TblUidInfo
            .AsNoTracking()
            .Include(u => u.OrderInfo)
            .FirstOrDefaultAsync(u =>
                (u.FirstLevelUid == uid || u.SecLevelUid == uid || u.ParentUid == uid) &&
                u.IsActive == true &&
                u.Used == false &&
                u.FkDestroyId == null &&
                u.CurrentStore != null &&
                validStoreIds.Contains(u.CurrentStore.Value), token);

        if (uidRecord == null)
            return (false, "UID یافت نشد", []);

        var order = uidRecord.OrderInfo;

        var childCount = await db.TblUidInfo.CountAsync(u =>
            u.ParentUid == uid &&
            u.IsActive == true &&
            u.Used == false &&
            u.FkDestroyId == null &&
            u.CurrentStore != null &&
            validStoreIds.Contains(u.CurrentStore.Value), token);

        var uidCount = childCount > 0 ? childCount : 1;

        var itemName = await db.TblItems
            .AsNoTracking()
            .Where(i => i.ItemId == order.FkItemId)
            .Select(i => i.FullName)
            .FirstOrDefaultAsync(token) ?? "-";

        var result = new DestroyUidSearchResultDto
        {
            PkOrderInfo = order.PkOrderInfo,
            SearchedUid = uid,   
            BatchNumber = order.BatchNumber,
            ItemName = itemName,
            DosePerVial = order.FkDosePerVialId,
            ProductionDate = order.ManufactureDate.ToShamsi(),
            ExpiryDate = order.ExpirationDate?.ToShamsi() ?? "-",
            UidCount = uidCount
        };

        return (true, string.Empty, [result]);
    }

    //public async Task<(bool Success, string Message)>
    //    SubmitDestroyAsync(List<DestroyUidSubmitDto> items, int userId, int userStoreId, CancellationToken token)
    //{
    //    var lastNumber = await db.TblDestroyUids.AnyAsync(token)
    //        ? await db.TblDestroyUids.MaxAsync(d => d.DestroyNumber, token)
    //        : 0;
    //    var destroyNumber = lastNumber + 1;
    //    var now = DateTime.Now;

    //    await using var transaction = await db.Database.BeginTransactionAsync(token);
    //    try
    //    {
    //        foreach (var item in items)
    //        {
    //            var uids = await db.TblUidInfo
    //                .Where(u =>
    //                    u.FkOrderInfo == item.PkOrderInfo &&
    //                    u.IsActive == true &&
    //                    u.Used == false &&
    //                    u.FkDestroyId == null &&
    //                    u.CurrentStore == userStoreId)
    //                .ToListAsync(token);

    //            foreach (var uid in uids)
    //            {
    //                var destroy = new TblDestroyUid
    //                {
    //                    FkUid = uid.PkUidInfo,
    //                    DestroyDate = item.DestroyDate,
    //                    DestroyNumber = destroyNumber,
    //                    FkLocationDestroy = userStoreId,
    //                    FkDestroyeBy = userId,
    //                    FkDestroyReasonId = item.DestroyReasonId,
    //                    CreationTime = now,
    //                    FkCreatedBy = userId
    //                };

    //                db.TblDestroyUids.Add(destroy);
    //                await db.SaveChangesAsync(token);

    //                uid.FkDestroyId = destroy.PkDestroyUid;
    //                await db.SaveChangesAsync(token);
    //            }
    //        }

    //        await transaction.CommitAsync(token);
    //        return (true, "امحا با موفقیت ثبت شد");
    //    }
    //    catch (Exception ex)
    //    {
    //        await transaction.RollbackAsync(token);
    //        return (false, "خطا در ثبت: " + ex.Message);
    //    }
    //}

    public async Task<(bool Success, string Message)>
    SubmitDestroyAsync(List<DestroyUidSubmitDto> items, int userId, int userStoreId, CancellationToken token)
    {
        var lastNumber = await db.TblDestroyUids.AnyAsync(token)
            ? await db.TblDestroyUids.MaxAsync(d => d.DestroyNumber, token)
            : 0;
        var destroyNumber = lastNumber + 1;
        var now = DateTime.Now;

        await using var transaction = await db.Database.BeginTransactionAsync(token);
        try
        {
            foreach (var item in items)
            {
                // ── یکجا بگیر، نه در حلقه ──
                var uids = await db.TblUidInfo
                    .Where(u =>
                        u.FkOrderInfo == item.PkOrderInfo &&
                        u.IsActive == true &&
                        u.Used == false &&
                        u.FkDestroyId == null &&
                        u.CurrentStore == userStoreId)
                    .ToListAsync(token);

                if (uids.Count == 0) continue;

                // ── Bulk insert: همه TblDestroyUid ها یکجا Add ──
                var destroys = uids.Select(uid => new TblDestroyUid
                {
                    FkUid = uid.PkUidInfo,
                    DestroyDate = item.DestroyDate,
                    DestroyNumber = destroyNumber,
                    FkLocationDestroy = userStoreId,
                    FkDestroyeBy = userId,
                    FkDestroyReasonId = item.DestroyReasonId,
                    CreationTime = now,
                    FkCreatedBy = userId
                }).ToList();

                db.TblDestroyUids.AddRange(destroys);

                await db.SaveChangesAsync(token);

                // ── حالا FkDestroyId را bulk update کن ──
                // destroys[i].PkDestroyUid بعد از SaveChanges پر شده
                for (int i = 0; i < uids.Count; i++)
                    uids[i].FkDestroyId = destroys[i].PkDestroyUid;

                // ── یک SaveChanges دیگه برای آپدیت UIDs ──
                await db.SaveChangesAsync(token);
            }

            await transaction.CommitAsync(token);
            return (true, "امحا با موفقیت ثبت شد");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            return (false, "خطا در ثبت: " + ex.Message);
        }
    }

    public async Task<int> CountDestroyableUidsAsync(
        List<DestroyUidSubmitDto> items, int userRecipientId, CancellationToken token)
    {
        var validStoreIds = await db.TblStores
            .Where(s => s.RecipientId == userRecipientId)
            .Select(s => s.StoreId)
            .ToListAsync(token);

        var orderIds = items.Select(i => i.PkOrderInfo).ToList();

        return await db.TblUidInfo
            .Where(u => orderIds.Contains(u.FkOrderInfo) &&
                        u.IsActive == true &&
                        u.Used == false &&
                        u.FkDestroyId == null &&
                        u.CurrentStore != null &&
                        validStoreIds.Contains(u.CurrentStore.Value))
            .CountAsync(token);
    }

    public async Task<(bool Success, string Message)> SubmitDestroyAsync(
        List<DestroyUidSubmitDto> items, int userId, int userRecipientId, CancellationToken token,
        Func<int, Task>? onProgress = null)
    {
        var validStoreIds = await db.TblStores
            .Where(s => s.RecipientId == userRecipientId)
            .Select(s => s.StoreId)
            .ToListAsync(token);

        if (validStoreIds.Count == 0)
            return (false, "انبار معتبری برای کاربر جاری یافت نشد");

        var lastNumber = await db.TblDestroyUids.AnyAsync(token)
            ? await db.TblDestroyUids.MaxAsync(d => d.DestroyNumber, token)
            : 0;
        var destroyNumber = lastNumber + 1;
        var now = DateTime.Now;
        int done = 0;

        await using var transaction = await db.Database.BeginTransactionAsync(token);
        try
        {
            foreach (var item in items)
            {
                var uids = await db.TblUidInfo
                  .Where(u => u.FkOrderInfo == item.PkOrderInfo &&
                              u.IsActive == true &&
                              u.Used == false &&
                              u.FkDestroyId == null &&
                              u.CurrentStore != null &&
                              validStoreIds.Contains(u.CurrentStore.Value) &&
                              (item.SearchedUid == null ||
                               u.ParentUid == item.SearchedUid ||
                               u.FirstLevelUid == item.SearchedUid ||
                               u.SecLevelUid == item.SearchedUid))
                  .ToListAsync(token);

                if (uids.Count == 0) continue;

                var destroys = uids.Select(uid => new TblDestroyUid
                {
                    FkUid = uid.PkUidInfo,
                    DestroyDate = item.DestroyDate,
                    DestroyNumber = destroyNumber,
                    FkLocationDestroy = userRecipientId,
                    FkDestroyeBy = userId,
                    FkDestroyReasonId = item.DestroyReasonId,
                    CreationTime = now,
                    FkCreatedBy = userId
                }).ToList();

                db.TblDestroyUids.AddRange(destroys);
                await db.SaveChangesAsync(token);

                for (int i = 0; i < uids.Count; i++)
                    uids[i].FkDestroyId = destroys[i].PkDestroyUid;

                await db.SaveChangesAsync(token);

                done += uids.Count;
                if (onProgress != null)
                    await onProgress(done);
            }

            await transaction.CommitAsync(token);
            return (true, "امحا با موفقیت ثبت شد");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            return (false, "خطا در ثبت: " + ex.Message);
        }
    }

}
