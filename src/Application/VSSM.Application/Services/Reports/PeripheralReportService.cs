using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.Reports
{
    public class PeripheralReportService(VSSMDbContext _context)
    {


        public void ExpiredItemsReport(int currentRecipientId)
        {
            var stores = _context.TblStores.Where(x => x.RecipientId == currentRecipientId);
            var itemDetails = _context.TblItemDetails.Where(x => x.EntryDate < DateTime.Now);

            var query = from i in _context.TblItems
                        join id in itemDetails on i.ItemId equals id.ItemId
                        join ids in _context.TblItemDetailStores on id.ItemDetailId equals ids.ItemDetailId
                        join s in stores on ids.StoreId equals s.StoreId
                        join vs in _context.TblVvmstages on ids.VvmstageId equals vs.VvmstageId
                        join cn in _context.TblCommercialNames on id.CommercialNameId equals cn.CommercialNameId
                        join dpv in _context.TblDosePerVials on id.DosePerVialId equals dpv.DosePerVialId
                        group new { i, /*id, s*/ } by new
                        {
                            i.ItemId,
                            //id.BatchNumber,
                            //s.StoreId
                        } into g
                        select new 
                        {

                        };

        }

    }
}
