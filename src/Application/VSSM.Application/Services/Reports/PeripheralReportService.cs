using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.Reports;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.Reports;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.Reports
{
    public class PeripheralReportService : IPeripheralReportService
    {
        private readonly VSSMDbContext _context;
        public PeripheralReportService(VSSMDbContext context)
        {
            _context = context;
        }


        public async Task<(List<ExpiredItemsReportDto> data, int count)> ExpiredItemsReport(
            int currentRecipientId,
            int pageNumber,
            int pageSize,
            string sort,
            string direction,
            CancellationToken cancellationToken = default)
        {

            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };

            var stores = _context.TblStores.Where(x => x.RecipientId == currentRecipientId);
            var itemDetails = _context.TblItemDetails.Where(x => x.EntryDate < DateTime.Now.Date);

            var query = from i in _context.TblItems
                        join id in itemDetails on i.ItemId equals id.ItemId
                        join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                        join s in stores on ids.StoreId equals s.StoreId
                        join vs in _context.TblVvmstages on ids.FkVvmStageId equals vs.VvmstageId
                        join cn in _context.TblCommercialNames on id.CommercialNameId equals cn.CommercialNameId
                        join dpv in _context.TblDosePerVials on id.DosePerVialId equals dpv.DosePerVialId
                        group new { i, id, s, cn, ids, dpv } by new
                        {
                            i.ItemId,
                            i.FullName,
                            id.ItemDetailId,
                            id.BatchNumber,
                            s.StoreId,
                            s.Name,
                        } into g
                        select new ExpiredItemsReportDto
                        {
                            ItemId = g.Key.ItemId,
                            ItemName = g.Key.FullName,
                            ItemDetailId = g.Key.ItemDetailId,
                            BatchNumber = g.Key.BatchNumber,
                            StoreId = g.Key.StoreId,
                            StoreName = g.Key.Name,

                            TotalStock = g.Sum(t => t.ids.CurrentStock),
                            DosePerVialCount = g.Sum(t => t.dpv.NumDosePerVial),
                            VialCount = g.Sum(t => t.ids.CurrentStock) / g.Sum(t => t.dpv.NumDosePerVial),
                            ExpiryDate = g.Max(t => t.id.ExpiryDate),
                            CommercialName = g.Max(t => t.cn.FullName),
                        };

            var total = await query.CountAsync();
            query = query.ApplySorting(sort, direction == "desc");

            return (await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(), total);

        }

    }
}
