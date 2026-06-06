using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Infrastructure.Services.BaseEntities;

using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.Interfaces.BaseEntities;

public sealed class UidInfoService : IUidInfoService
{
    private readonly VSSMDbContext _db;
    public UidInfoService(VSSMDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<List<UidInfoDto>> GetAvailableByBatchNum(string batchNum, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(batchNum))
            throw new ArgumentException("سری ساخت نامعتبر است", nameof(batchNum));

        batchNum = batchNum.Trim();

        // 1) آیا فایل/سفارش تایید شده برای این Batch وجود دارد؟
        var hasConfirmedOrder = await _db.TblOrderInfo
            .AsNoTracking()
            .AnyAsync(o => o.BatchNumber == batchNum, token);

        if (!hasConfirmedOrder)
            throw new InvalidOperationException("فایل مربوط به سری ساخت مورد نظر آپلود نشده است");

        // 2) UID آزاد (ورود نخورده) داریم؟
        var list = await _db.TblUidInfo
            .Include(t => t.OrderInfo)
            .AsNoTracking()
            .Where(u =>
                u.IsActive &&
                u.OrderInfo.FkArrivalId == null &&
                u.OrderInfo.BatchNumber == batchNum)
            .Select(u => new UidInfoDto
            {
                Id = u.PkUidInfo,
                BatchNumber = u.OrderInfo.BatchNumber,
                ArrivalFkId = u.OrderInfo.FkArrivalId,
                ItemFkId = u.OrderInfo.FkItemId,
                StoreCurrent = u.CurrentStore,
                DoseCount = u.DoseCount
            })
            .ToListAsync(token);

        if (list.Count == 0)
            throw new InvalidOperationException("کلیه ویالهای مربوط به سری ساخت وارد شده ورود خورده‌اند");

        return list;
    }

    public async Task SetArrivalFkForBatchNums(
      List<string> batchNums,
      int arrivalId,
      int? doseCount,
      int? storeId,
      int? itemId,
      CancellationToken token)
    {
        if (arrivalId <= 0)
            throw new ArgumentOutOfRangeException(nameof(arrivalId));

        if (batchNums == null || batchNums.Count == 0)
            throw new ArgumentException("لیست سری ساخت خالی است", nameof(batchNums));

        var normalized = batchNums
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
            throw new ArgumentException("لیست سری ساخت معتبر نیست", nameof(batchNums));

        await using var transaction = await _db.Database.BeginTransactionAsync(token);

        try
        {
            var affectedOrders = await _db.TblOrderInfo
                .Where(o =>
                    o.FkArrivalId == null &&
                    normalized.Contains(o.BatchNumber) &&
                    o.TblUidInfos.Any(u => u.IsActive))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.FkArrivalId, arrivalId)
                    .SetProperty(o => o.FkItemId, itemId),
                    token);

            // 2) آپدیت TblUidInfo
            var affectedUids = await _db.TblUidInfo
                .Where(u =>
                    u.IsActive &&
                    u.OrderInfo.FkArrivalId == arrivalId &&
                    normalized.Contains(u.OrderInfo.BatchNumber))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.CurrentStore, storeId)
                    .SetProperty(u => u.DoseCount, doseCount),
                    token);

            if (affectedOrders == 0 && affectedUids == 0)
                throw new InvalidOperationException("هیچ UID آزادی برای سری ساخت(های) انتخاب‌شده جهت ورود یافت نشد.");

            await transaction.CommitAsync(token);
        }
        catch
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }




    public async Task ClearArrivalFkByArrivalId(int arrivalId, CancellationToken token)
    {
        if (arrivalId <= 0)
            throw new ArgumentOutOfRangeException(nameof(arrivalId));

        await using var transaction = await _db.Database.BeginTransactionAsync(token);

        try
        {
            var orderIds = await _db.TblOrderInfo
                .Where(o => o.FkArrivalId == arrivalId)
                .Select(o => o.PkOrderInfo)
                .Distinct()
                .ToListAsync(token);

            if (orderIds.Count == 0)
                return;

            await _db.TblOrderInfo
                .Where(o => orderIds.Contains(o.PkOrderInfo))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.FkArrivalId, (int?)null)
                    .SetProperty(o => o.FkItemId, (int?)null),
                    token);

            await _db.TblUidInfo
                .Where(u => u.FkOrderInfo != null && orderIds.Contains(u.FkOrderInfo))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.CurrentStore, (int?)null)
                    .SetProperty(u => u.DoseCount, (int?)null),
                    token);

            await transaction.CommitAsync(token);
        }
        catch
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }


    public async Task ClearArrivalFkByLineId(int arrivalLineId, CancellationToken token)
    {
        var uidsToUpdate = await _db.TblUidInfo.Include(t => t.OrderInfo)
            .Where(x => x.OrderInfo.FkArrivalId == arrivalLineId)
            .ToListAsync(token);

        if (uidsToUpdate.Any())
        {
            foreach (var uid in uidsToUpdate)
            {
                uid.OrderInfo.FkArrivalId = null;
            }

            await _db.SaveChangesAsync(token);
        }
    }

}

