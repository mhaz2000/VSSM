using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Infrastructure.Services.BaseEntities;

using Microsoft.EntityFrameworkCore;
using VSSM.Application.Interfaces.BaseEntities;

public sealed class UidInfoService(VSSMDbContext dbContext) : IUidInfoService
{
    public async Task SetArrivalFkForBatchNums(
        List<string> batchNums,
        int arrivalId,
        int? doseCount,
        int? storeId,
        int? itemId,
        bool? isHalal,
        CancellationToken token)
    {
        if (arrivalId <= 0) throw new ArgumentOutOfRangeException(nameof(arrivalId));
        if (batchNums == null || batchNums.Count == 0)
            throw new ArgumentException("لیست سری ساخت خالی است", nameof(batchNums));

        var normalized = batchNums
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
            throw new ArgumentException("لیست سری ساخت معتبر نیست", nameof(batchNums));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(token);
        try
        {
            await dbContext.TblOrderInfo
                .Where(o =>
                    normalized.Contains(o.BatchNumber) &&
                    o.TblUidInfos.Any(u => u.IsActive))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.FkArrivalId, arrivalId)
                    .SetProperty(o => o.FkItemId, itemId),
                    token);

            var affectedUids = await dbContext.TblUidInfo
                .Where(u =>
                    u.IsActive &&
                    normalized.Contains(u.OrderInfo.BatchNumber))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.CurrentStore, storeId)
                    .SetProperty(u => u.DoseCount, doseCount),
                    token);

            if (!isHalal.HasValue && affectedUids == 0)
                throw new InvalidOperationException(
                    "هیچ UID فعالی برای سری ساخت‌های انتخاب‌شده یافت نشد.");

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

        await using var transaction = await dbContext.Database.BeginTransactionAsync(token);

        try
        {
            var orderIds = await dbContext.TblOrderInfo
                .Where(o => o.FkArrivalId == arrivalId)
                .Select(o => o.PkOrderInfo)
                .Distinct()
                .ToListAsync(token);

            if (orderIds.Count == 0)
                return;

            await dbContext.TblOrderInfo
                .Where(o => orderIds.Contains(o.PkOrderInfo))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.FkArrivalId, (int?)null)
                    .SetProperty(o => o.FkItemId, (int?)null),
                    token);

            await dbContext.TblUidInfo
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

    public async Task<List<UidInfoDto>> GetAvailableByBatchNum(
    string batchNum,
    int recipientId,
    CancellationToken token)
    {
        batchNum = batchNum.Trim();

        // اول StoreId های متعلق به این recipientId رو بگیر
        var validStoreIds = await dbContext.TblStores
            .Where(s => s.RecipientId == recipientId)
            .Select(s => s.Id)
            .ToListAsync(token);

        var list = await dbContext.TblUidInfo
            .Include(t => t.OrderInfo)
            .AsNoTracking()
            .Where(u =>
                u.IsActive &&
                u.OrderInfo.FkArrivalId == null &&
                u.OrderInfo.BatchNumber == batchNum &&
                (u.CurrentStore == null || validStoreIds.Contains(u.CurrentStore.Value)))
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

        return list;
    }
    public async Task ClearArrivalFkByLineId(int arrivalLineId, CancellationToken token)
    {
        var uidsToUpdate = await dbContext.TblUidInfo.Include(t => t.OrderInfo)
            .Where(x => x.OrderInfo.FkArrivalId == arrivalLineId)
            .ToListAsync(token);

        if (uidsToUpdate.Any())
        {
            foreach (var uid in uidsToUpdate)
            {
                uid.OrderInfo.FkArrivalId = null;
            }

            await dbContext.SaveChangesAsync(token);
        }
    }

}

