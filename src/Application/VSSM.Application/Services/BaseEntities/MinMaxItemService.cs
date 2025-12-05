// Application/Services/BaseEntities/MinMaxItemService.cs
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.MinMaxItem;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class MinMaxItemService : IMinMaxItemService
    {
        private readonly VSSMDbContext _db;

        public MinMaxItemService(VSSMDbContext db)
        {
            _db = db;
        }

        public async Task<PagingSearchOrderDto<MinMaxItemListDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken token)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            var query = _db.TblMinMaxItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim();

                query =
                    from m in query
                    join i in _db.TblItems on m.ItemId equals i.ItemId into gj
                    from i in gj.DefaultIfEmpty()
                    where i.FullName.Contains(normalized) || i.Code.Contains(normalized)
                    select m;
            }

            // sorting
            query = sortField switch
            {
                "Code" => (sortOrder == "asc"
                        ? query.OrderBy(x => x.ItemId)
                        : query.OrderByDescending(x => x.ItemId)),
                "EntryDate" => (sortOrder == "asc"
                        ? query.OrderBy(x => x.EntryDate)
                        : query.OrderByDescending(x => x.EntryDate)),
                _ => query.OrderBy(x => x.MinMaxItemId)
            };

            paging.TotalItems = await query.CountAsync(token);

            var list = await
                (from m in query
                 join i in _db.TblItems on m.ItemId equals i.ItemId into gi
                 from i in gi.DefaultIfEmpty()
                 join r in _db.TblRecipients on m.RecipientId equals r.RecipientId into gr
                 from r in gr.DefaultIfEmpty()
                 select new MinMaxItemListDto
                 {
                     MinMaxItemId = m.MinMaxItemId,
                     ItemId = m.ItemId,
                     ItemName = i.FullName,
                     RecipientId = m.RecipientId,
                     RecipientName = r.FullName,
                     TypeOfRoadId = r.TypeOfRoadId,

                     EntryDate = m.EntryDate.HasValue ? DateTimeHelper.ToShamsi(m.EntryDate.Value) : string.Empty,
                 })
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .ToListAsync(token);

            return new PagingSearchOrderDto<MinMaxItemListDto>
            {
                Items = list,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<MinMaxItemGetDto?> GetById(int id, CancellationToken token)
        {
            var e = await _db.TblMinMaxItems
                .FirstOrDefaultAsync(x => x.MinMaxItemId == id, token);

            if (e == null) return null;

            return new MinMaxItemGetDto
            {
                MinMaxItemId = e.MinMaxItemId,
                ItemId = e.ItemId,
                RecipientId = e.RecipientId,
                StockMin = e.StockMin,
                StockMax = e.StockMax,
                Min = e.Min,
                Max = e.Max,
                AnnualNeeds = e.AnnualNeeds,

                NumberShipment = e.NumberShipment,
                PercentSafety = e.PercentSafety,
                LeadConsumption = e.LeadConsumption,
                TotalPop = e.TotalPop,
                PercentUnder = e.PercentUnder,
                NumDose = e.NumDose,
                Wastage = e.Wastage,
                PercentCoverage = e.PercentCoverage,
                EntryDate = e.EntryDate,
                OrderLevel = e.OrderLevel
            };
        }

        public async Task<int> Create(MinMaxItemCreateDto dto, CancellationToken token)
        {
            var now = DateTime.Now;

            var e = new TblMinMaxItem
            {
                ItemId = dto.ItemId,
                RecipientId = dto.RecipientId,
                StockMin = dto.StockMin,
                StockMax = dto.StockMax,
                Min = dto.Min,
                Max = dto.Max,
                AnnualNeeds = dto.AnnualNeeds,

                NumberShipment = dto.NumberShipment,
                PercentSafety = dto.PercentSafety,
                LeadConsumption = dto.LeadConsumption,
                TotalPop = dto.TotalPop,
                PercentUnder = dto.PercentUnder,
                NumDose = dto.NumDose,
                Wastage = dto.Wastage,
                PercentCoverage = dto.PercentCoverage,
                EntryDate = now,
                OrderLevel = dto.OrderLevel
            };

            _db.TblMinMaxItems.Add(e);
            await _db.SaveChangesAsync(token);

            return e.MinMaxItemId;
        }

        public async Task Update(MinMaxItemEditDto dto, CancellationToken token)
        {
            var e = await _db.TblMinMaxItems
                .FirstOrDefaultAsync(x => x.MinMaxItemId == dto.MinMaxItemId, token);

            if (e == null) return;

            e.ItemId = dto.ItemId;
            e.RecipientId = dto.RecipientId;
            e.StockMin = dto.StockMin;
            e.StockMax = dto.StockMax;
            e.Min = dto.Min;
            e.Max = dto.Max;
            e.AnnualNeeds = dto.AnnualNeeds;

            e.NumberShipment = dto.NumberShipment;
            e.PercentSafety = dto.PercentSafety;
            e.LeadConsumption = dto.LeadConsumption;
            e.TotalPop = dto.TotalPop;
            e.PercentUnder = dto.PercentUnder;
            e.NumDose = dto.NumDose;
            e.Wastage = dto.Wastage;
            e.PercentCoverage = dto.PercentCoverage;
            e.OrderLevel = dto.OrderLevel;

            await _db.SaveChangesAsync(token);
        }

        public async Task Delete(int id, CancellationToken token)
        {
            var e = await _db.TblMinMaxItems
                .FirstOrDefaultAsync(x => x.MinMaxItemId == id, token);
            if (e == null) return;

            _db.TblMinMaxItems.Remove(e);
            await _db.SaveChangesAsync(token);
        }
    }
}
