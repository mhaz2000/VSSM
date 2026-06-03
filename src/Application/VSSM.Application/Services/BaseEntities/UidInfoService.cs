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
            .AsNoTracking()
            .Where(u =>
                u.IsActive &&
                u.FkArrivalId == null &&
                u.OrderInfo.BatchNumber == batchNum)
            .Select(u => new UidInfoDto
            {
                Id = u.PkUidInfo,
                BatchNumber = u.OrderInfo.BatchNumber,
                ArrivalFkId = u.FkArrivalId,
                ItemFkId = u.FkItemId,
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

        // نکته: این آپدیت فقط UIDهای آزاد را می‌گیرد (ArrivalFkId == null)
        // و فقط برای سفارش‌های تایید شده
        var affected = await _db.TblUidInfo
            .Where(u =>
                u.IsActive &&
                u.FkArrivalId == null &&
                normalized.Contains(u.OrderInfo.BatchNumber))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.FkArrivalId, arrivalId)
                .SetProperty(u => u.FkItemId, itemId)
                .SetProperty(u => u.CurrentStore, storeId)
                .SetProperty(u => u.DoseCount, doseCount),
                token);

        // اگر می‌خواهی وقتی هیچ UID آپدیت نشد خطا بدهی:
        if (affected == 0)
            throw new InvalidOperationException("هیچ UID آزادی برای سری ساخت(های) انتخاب‌شده جهت ورود یافت نشد.");
    }

    public async Task ClearArrivalFkByArrivalId(int arrivalId, CancellationToken token)
    {
        if (arrivalId <= 0)
            throw new ArgumentOutOfRangeException(nameof(arrivalId));

        await _db.TblUidInfo
            .Where(u => u.FkArrivalId == arrivalId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.FkArrivalId, (int?)null)
                .SetProperty(u => u.FkItemId, (int?)null)
                .SetProperty(u => u.CurrentStore, (int?)null)
                .SetProperty(u => u.DoseCount, (int?)null),
                token);
    }

    public async Task ClearArrivalFkByLineId(int arrivalLineId, CancellationToken token)
    {
        var uidsToUpdate = await _db.TblUidInfo
            .Where(x => x.FkArrivalId == arrivalLineId)
            .ToListAsync(token);

        if (uidsToUpdate.Any())
        {
            foreach (var uid in uidsToUpdate)
            {
                uid.FkArrivalId = null;
            }

            await _db.SaveChangesAsync(token);
        }
    }

}

