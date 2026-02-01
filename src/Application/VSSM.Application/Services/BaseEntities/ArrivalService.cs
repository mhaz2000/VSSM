using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VSSM.Application.Services.BaseEntities
{
    public class ArrivalService : IArrivalService
    {
        private readonly VSSMDbContext _db;

        public ArrivalService(VSSMDbContext db)
        {
            _db = db;
        }

        /* =========================
           GRID
        ========================= */
        public async Task<PagingSearchOrderDto<ArrivalListDto>> GetPagedList(
            string? search,
            int statusFilter,
            int page,
            int pageSize,
            CancellationToken token)
        {
            var query = _db.TblArrivals.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.ReportNumber.Contains(search));


            if (statusFilter == 1) // Draft
                query = query.Where(x => x.IsDrafted == true && x.IsCancelled != true);
            else if (statusFilter == 2) // Confirmed
                query = query.Where(x => x.IsConfirmed == true && x.IsCancelled != true);

            var paging = new PagingModel
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = await query.CountAsync(token)
            };

            var list = await query
                .OrderByDescending(x => x.Date)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new ArrivalListDto
                {
                    ReportNumber = x.ReportNumber,
                    ArrivalId = x.ArrivalId,
                    Date = x.Date,
                    TotalVolume = x.TotalVolume,
                    IsConfirmed = x.IsConfirmed,
                    IsDrafted = x.IsDrafted,
                    IsCancelled = x.IsCancelled,
                    SourceName = x.SourceId != null
                        ? _db.TblSources.Where(s => s.SourceId == x.SourceId).Select(s => s.FullName).FirstOrDefault()
                        : null,
                    FiscalPeriodName = x.FiscalPeriodId != null
                        ? _db.TblFiscalPeriods.Where(f => f.FiscalPeriodId == x.FiscalPeriodId).Select(f => f.FullName)
                            .FirstOrDefault()
                        : null
                })
                .ToListAsync(token);

            return new PagingSearchOrderDto<ArrivalListDto>
            {
                Items = list,
                Paging = paging,
                SearchText = search,
                SortField = "Date",
                SortOrder = "desc"
            };
        }

        /* =========================
           GET BY ID
        ========================= */
        public async Task<ArrivalGetDto?> GetById(int id, CancellationToken token)
        {
            var arrival = await _db.TblArrivals
                .Include(x => x.TblArrivalLines)
                .FirstOrDefaultAsync(x => x.ArrivalId == id, token);

            if (arrival == null) return null;

            return new ArrivalGetDto
            {
                ArrivalId = arrival.ArrivalId,
                Date = arrival.Date,
                ReportNumber = arrival.ReportNumber,
                SourceId = arrival.SourceId,
                FiscalPeriodId = arrival.FiscalPeriodId,
                EntryDate = arrival.EntryDate,
                IsConfirmed = arrival.IsConfirmed,
                IsDrafted = arrival.IsDrafted,
                IsCancelled = arrival.IsCancelled,
                PackedVolume = arrival.PackedVolume,
                TotalWeight = arrival.TotalWeight,
                TotalVolume = arrival.TotalVolume,
                Note = arrival.Note,
                Frzindc = arrival.Frzindc,
                FrzindcNok = arrival.FrzindcNok,
                Lines = arrival.TblArrivalLines
                    .Where(l => l.ArrivalId == arrival.ArrivalId)
                    .Select(l => new ArrivalLineDto
                    {
                        ArrivalLineId = l.ArrivalLineId,
                        ArrivalId = l.ArrivalId,
                        ItemId = l.ItemId,
                        BatchNum = l.BatchNum,
                        CommertialNameId = l.CommertialNameId,
                        DosePerVialId = l.DosePerVialId,
                        ExpiryDate = l.ExpiryDate,
                        ProductionDate = l.ProductionDate,
                        ItemDetailId = l.ItemDetailId,
                        ManufacturerId = l.ManufacturerId,
                        Note = l.Note,
                        NumVials = l.NumVials,
                        PackageSize = l.PackageSize,
                        PackingTypeId = l.PackingTypeId,
                        Quantity = l.Quantity,
                        StoreId = l.StoreId,
                        TotalVolume = l.TotalVolume,
                        UnitVolume = l.UnitVolume,
                        UseId = l.UseId,
                        VvmStageId = l.VvmstageId,
                        VvmTypeId = l.VvmtypeId
                    }).ToList()
            };
        }

        /* =========================
           CREATE HEADER ONLY
        ========================= */
        public async Task<int> Create(ArrivalCreateDto dto, CancellationToken token)
        {
            var arrival = new TblArrival
            {
                Date = dto.Date,
                ReportNumber = dto.ReportNumber,
                SourceId = dto.SourceId,
                FiscalPeriodId = dto.FiscalPeriodId,
                EntryDate = DateTime.Now,
                IsDrafted = true,
                IsConfirmed = false,
                IsCancelled = false,
                PackedVolume = dto.PackedVolume,
                TotalWeight = dto.TotalWeight,
                TotalVolume = dto.TotalVolume,
                Note = dto.Note,
                Frzindc = dto.Frzindc,
                FrzindcNok = dto.FrzindcNok,
                ReceiverId = dto.UserRecipientId,
                EnteredById = dto.EnterById
            };

            _db.TblArrivals.Add(arrival);
            await _db.SaveChangesAsync(token);
            return arrival.ArrivalId;
        }


        /* =========================
           CONFIRM
        ========================= */
        public async Task Confirm(int arrivalId, ArrivalCreateDto dto, CancellationToken token)
        {
            var arrival = await _db.TblArrivals.FindAsync(new object[] { arrivalId }, token);


            if (arrival == null) return;

            arrival.ReportNumber = dto.ReportNumber;
            arrival.SourceId = dto.SourceId;
            arrival.EntryDate = DateTime.Now;
            arrival.IsCancelled = false;
            arrival.PackedVolume = dto.PackedVolume;
            arrival.TotalWeight = dto.TotalWeight;
            arrival.TotalVolume = dto.TotalVolume;
            arrival.Note = dto.Note;
            arrival.Frzindc = dto.Frzindc;
            arrival.FrzindcNok = dto.FrzindcNok;
            arrival.ReceiverId = dto.UserRecipientId;
            arrival.IsConfirmed = true;
            arrival.IsDrafted = false;
            arrival.ConfirmationTime = DateTime.Now;

            await _db.SaveChangesAsync(token);
        }

        /* =========================
           DRAFT
        ========================= */
        public async Task SetDraft(int arrivalId, ArrivalCreateDto dto, CancellationToken token)
        {
            var arrival = await _db.TblArrivals.FindAsync(new object[] { arrivalId }, token);
            if (arrival == null) return;

            arrival.ReportNumber = dto.ReportNumber;
            arrival.SourceId = dto.SourceId;
            arrival.EntryDate = DateTime.Now;
            arrival.IsCancelled = false;
            arrival.PackedVolume = dto.PackedVolume;
            arrival.TotalWeight = dto.TotalWeight;
            arrival.TotalVolume = dto.TotalVolume;
            arrival.Note = dto.Note;
            arrival.Frzindc = dto.Frzindc;
            arrival.FrzindcNok = dto.FrzindcNok;
            arrival.ReceiverId = dto.UserRecipientId;
            arrival.IsDrafted = true;
            arrival.IsConfirmed = false;

            await _db.SaveChangesAsync(token);
        }

        /* =========================
           DELETE WHOLE ARRIVAL
        ========================= */
        public async Task Cancel(int id, CancellationToken token)
        {
            var arrival = await _db.TblArrivals.FindAsync(new object[] { id }, token);
            if (arrival == null) return;

            arrival.IsCancelled = true;
            await _db.SaveChangesAsync(token);
        }

        /* =========================
           DELETE ONE LINE
        ========================= */
        public async Task CancelLine(int arrivalLineId, CancellationToken token)
        {
            var line = await _db.TblArrivalLines.FindAsync(new object[] { arrivalLineId }, token);
            if (line == null) return;

            _db.TblArrivalLines.Remove(line);
            await _db.SaveChangesAsync(token);
        }

        public async Task<List<ArrivalCopyDto>> GetForCopy(
            string? reportNumber,
            CancellationToken token)
        {
            var query = _db.TblArrivals
                .Where(x => x.IsCancelled != true); // ⬅ only cancelled excluded

            if (!string.IsNullOrWhiteSpace(reportNumber))
            {
                query = query.Where(x => x.ReportNumber.Contains(reportNumber));
            }

            return await query
                .OrderByDescending(x => x.EntryDate)
                .Select(x => new ArrivalCopyDto
                {
                    ArrivalId = x.ArrivalId,
                    DisplayText =
                        _db.TblFiscalPeriods
                            .Where(f => f.FiscalPeriodId == x.FiscalPeriodId)
                            .Select(f => f.FullName)
                            .FirstOrDefault()
                        + " - " + x.ReportNumber
                })
                .ToListAsync(token);
        }

        public async Task AddToTotal(int? id, float total)
        {
            (await _db.TblArrivals.FirstOrDefaultAsync(z => z.ArrivalId == id)).TotalVolume += total;
            await _db.SaveChangesAsync();
        }

        public async Task SubtractionToTotal(int lineId, int arrivalId)
        {
            (await _db.TblArrivals.FirstOrDefaultAsync(z => z.ArrivalId == arrivalId)).TotalVolume -=
                _db.TblArrivalLines.FirstOrDefault(z => z.ArrivalLineId == lineId).TotalVolume;
            await _db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await _db.TblArrivals.FirstOrDefaultAsync(z => z.ArrivalId == id);
            _db.TblArrivals.Remove(entity);
            await _db.SaveChangesAsync();
        }

        /* =========================
   ADD LINE (INCREMENTAL)
========================= */
        public async Task AddLine(int arrivalId, ArrivalLineDto dto, CancellationToken token)
        {
            var does = _db.TblDosePerVials.First(z => z.DosePerVialId == dto.DosePerVialId).NumDosePerVial;
            if (dto.Quantity % does != 0)
                throw new ValidationException("تعداد باید مضربی از دوز بر ویال باشد");
            var id = _db.TblArrivalLines.OrderByDescending(z => z.ArrivalLineId).First().ArrivalLineId + 1;
            var line = new TblArrivalLine
            {
                ArrivalLineId = id,
                ArrivalId = arrivalId,
                ItemId = dto.ItemId,
                BatchNum = dto.BatchNum,
                CommertialNameId = dto.CommertialNameId,
                DosePerVialId = dto.DosePerVialId,
                ExpiryDate = dto.ExpiryDate,
                ProductionDate = dto.ProductionDate,
                ItemDetailId = dto.ItemDetailId,
                ManufacturerId = dto.ManufacturerId,
                Note = dto.Note,
                NumVials = dto.NumVials,
                PackageSize = dto.PackageSize,
                PackingTypeId = dto.PackingTypeId,
                Quantity = dto.Quantity,
                StoreId = dto.StoreId,
                UnitVolume = dto.UnitVolume,
                TotalVolume = dto.TotalVolume,
                UseId = dto.UseId,
                VvmstageId = dto.VvmStageId,
                VvmtypeId = dto.VvmTypeId,
                EntryDate = DateTime.Now
            };

            _db.TblArrivalLines.Add(line);

            // update header total volume
            var arrival = await _db.TblArrivals.FindAsync(new object[] { arrivalId }, token);
            arrival.TotalVolume = await _db.TblArrivalLines
                .Where(x => x.ArrivalId == arrivalId)
                .SumAsync(x => x.TotalVolume ?? 0, token);

            await _db.SaveChangesAsync(token);
        }

        public async Task<List<ArrivalLineDto>> GetArrivalLines(int userRecipientId)
        {
            var lines = await _db.TblArrivalLines
                .Where(l => _db.TblArrivals
                    .Any(a => a.ReceiverId == userRecipientId && a.ArrivalId == l.ArrivalId))
                .Select(l => new
                {
                    Line = l,
                    Item = _db.TblItems.FirstOrDefault(i => i.ItemId == l.ItemId),
                    Manufacturer = _db.TblManufacturers.FirstOrDefault(m => m.ManufacturerId == l.ManufacturerId),
                    Store = _db.TblStores.FirstOrDefault(s => s.StoreId == l.StoreId),
                    DoseVial = _db.TblDosePerVials.FirstOrDefault(d => d.DosePerVialId == l.DosePerVialId)
                })
                .AsNoTracking()
                .ToListAsync();

            var result = lines.Select(x =>
            {
                var l = x.Line;

                return new ArrivalLineDto
                {
                    ArrivalLineId = l.ArrivalLineId,
                    ArrivalId = l.ArrivalId,
                    ItemId = l.ItemId,
                    BatchNum = l.BatchNum,
                    CommertialNameId = l.CommertialNameId,
                    DosePerVialId = l.DosePerVialId,
                    ExpiryDate = l.ExpiryDate,
                    ProductionDate = l.ProductionDate,
                    ItemDetailId = l.ItemDetailId,
                    ManufacturerId = l.ManufacturerId,
                    Note = l.Note,
                    NumVials = l.NumVials,
                    PackageSize = l.PackageSize,
                    PackingTypeId = l.PackingTypeId,
                    Quantity = l.Quantity,
                    StoreId = l.StoreId,
                    TotalVolume = l.TotalVolume,
                    UnitVolume = l.UnitVolume,
                    UseId = l.UseId,
                    VvmStageId = l.VvmstageId,
                    VvmTypeId = l.VvmtypeId,
                    DropDownText = string.Format(
                        "کالا: {0} | سازنده: {1} | تاریخ انقضا: {2} | سری ساخت: {3} | انبار: {4} | دوز بر ویال: {5} | مقدار موجود: {6}",
                        x.Item != null ? x.Item.FullName : "",
                        x.Manufacturer != null ? x.Manufacturer.FullName : "",
                        l.ExpiryDate != null ? l.ExpiryDate.Value.ToShamsi() : "",
                        l.BatchNum,
                        x.Store != null ? x.Store.Name : "",
                        x.DoseVial != null ? x.DoseVial.NumDosePerVial : "",
                        l.Quantity
                    )
                };
            }).ToList();

            return result;
        }

        public async Task<bool> DoesHaveArrivalLineByArrivalId(int id)
        {

            if (await _db.TblArrivalLines.AnyAsync(c => c.ArrivalId == id))
                return true;
            return false;



        }
        public async Task<ArrivalLineDto> GetArrivalLineById(int id)
        {
            var line = await _db.TblArrivalLines.FirstOrDefaultAsync(c => c.ArrivalLineId == id);

            var dto = new ArrivalLineDto
            {
                ArrivalLineId = line.ArrivalLineId,
                ArrivalId = line.ArrivalId,
                ItemId = line.ItemId,
                BatchNum = line.BatchNum,
                CommertialNameId = line.CommertialNameId,
                DosePerVialId = line.DosePerVialId,
                ExpiryDate = line.ExpiryDate,
                ProductionDate = line.ProductionDate,
                ItemDetailId = line.ItemDetailId,
                ManufacturerId = line.ManufacturerId,
                Note = line.Note,
                NumVials = line.NumVials,
                PackageSize = line.PackageSize,
                PackingTypeId = line.PackingTypeId,
                Quantity = line.Quantity,
                StoreId = line.StoreId,
                TotalVolume = line.TotalVolume,
                UnitVolume = line.UnitVolume,
                UseId = line.UseId,
                VvmStageId = line.VvmstageId,
                VvmTypeId = line.VvmtypeId
            };

            return dto;
        }

        public async Task<ArrivalLineDto> GetArrivalLineByStoreIdAndItemDetailId(long storeId, long itemDetailId)
        {
            var line = await _db.TblArrivalLines.FirstOrDefaultAsync(c => c.StoreId == storeId && c.ItemDetailId == itemDetailId);

            var dto = new ArrivalLineDto
            {
                ArrivalLineId = line.ArrivalLineId,
                ArrivalId = line.ArrivalId,
                ItemId = line.ItemId,
                BatchNum = line.BatchNum,
                CommertialNameId = line.CommertialNameId,
                DosePerVialId = line.DosePerVialId,
                ExpiryDate = line.ExpiryDate,
                ProductionDate = line.ProductionDate,
                ItemDetailId = line.ItemDetailId,
                ManufacturerId = line.ManufacturerId,
                Note = line.Note,
                NumVials = line.NumVials,
                PackageSize = line.PackageSize,
                PackingTypeId = line.PackingTypeId,
                Quantity = line.Quantity,
                StoreId = line.StoreId,
                TotalVolume = line.TotalVolume,
                UnitVolume = line.UnitVolume,
                UseId = line.UseId,
                VvmStageId = line.VvmstageId,
                VvmTypeId = line.VvmtypeId
            };

            return dto;
        }
    }
}