using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VSSM.Application.DTOs.Dashboard;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services;

public class DashboardService(
    VSSMDbContext context,
    ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardDataAsync(int recipientId)
    {
        var viewModel = new DashboardViewModel();

        try
        {
            viewModel.ArrivedShipments = await context.TblArrivals
                .AsNoTracking()
                .Where(a => a.ConfirmationTime != null)
                .CountAsync();

            var draftStatus = RequestStatusEnum.Draft;

            viewModel.DraftOutgoing = await context.TblRequests
                .AsNoTracking()
                .Where(r =>
                    r.Status == draftStatus &&
                    r.SenderId == recipientId)
                .CountAsync();

            viewModel.DraftIncoming = await context.TblRequests
                .AsNoTracking()
                .Where(r =>
                    r.Status == draftStatus &&
                    r.ReceiverId == recipientId)
                .CountAsync();

            viewModel.BelowMinItems = await context.TblItemDetailStores
                .AsNoTracking()
                .Join(
                    context.TblMinMaxItems.AsNoTracking(),
                    ids => ids.ItemDetail.ItemId,
                    mm => mm.ItemId,
                    (ids, mm) => new { ids, mm })
                .Where(x =>
                    x.mm.RecipientId == recipientId &&
                    x.ids.CurrentStock < x.mm.Min)
                .GroupBy(x => x.ids.ItemDetail.Item.FullName)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToListAsync();

            viewModel.BelowRequestItems = await context.TblItemDetailStores
                .AsNoTracking()
                .Join(
                    context.TblMinMaxItems.AsNoTracking(),
                    ids => ids.ItemDetail.ItemId,
                    mm => mm.ItemId,
                    (ids, mm) => new { ids, mm })
                .Where(x =>
                    x.mm.RecipientId == recipientId &&
                    x.ids.CurrentStock < x.mm.Max)
                .GroupBy(x => x.ids.ItemDetail.Item.FullName)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToListAsync();

            var now = DateTime.Now;
            var expiryThreshold = now.AddMonths(3);

            viewModel.NearExpiryItems = await context.TblItemDetails
                .AsNoTracking()
                .Join(
                    context.TblItemDetailStores.AsNoTracking(),
                    id => id.ItemDetailId,
                    ids => ids.ItemDetailId,
                    (id, ids) => new { id, ids })
                .Where(x =>
                    x.id.ExpiryDate <= expiryThreshold &&
                    x.id.ExpiryDate >= now)
                .GroupBy(x => x.id.Item.FullName)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToListAsync();

            viewModel.AboveMaxItems = await context.TblItemDetailStores
                .AsNoTracking()
                .Join(
                    context.TblMinMaxItems.AsNoTracking(),
                    ids => ids.ItemDetail.ItemId,
                    mm => mm.ItemId,
                    (ids, mm) => new { ids, mm })
                .Where(x =>
                    x.mm.RecipientId == recipientId &&
                    x.ids.CurrentStock > x.mm.Max)
                .GroupBy(x => x.ids.ItemDetail.Item.FullName)
                .Select(g => new ChartData
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToListAsync();

            return viewModel;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "خطا در دریافت داده‌های داشبورد برای RecipientId: {RecipientId}",
                recipientId);

            throw;
        }

    }
}