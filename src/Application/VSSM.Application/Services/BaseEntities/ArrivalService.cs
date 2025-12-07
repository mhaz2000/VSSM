using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSSM.Application.DTOs.Arrivals;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services
{
    public class ArrivalService : IArrivalService
    {
        private readonly VSSMDbContext _ctx;

        public ArrivalService(VSSMDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IList<ArrivalListDto>> GetList(
            string search,
            ArrivalStatusFilter status,
            int page,
            int pageSize,
            CancellationToken token)
        {
            var q = _ctx.TblArrivals.AsQueryable();

            // فیلتر وضعیت
            switch (status)
            {
                case ArrivalStatusFilter.Draft:
                    q = q.Where(a => a.IsDrafted == true && a.IsCancelled != true);
                    break;
                case ArrivalStatusFilter.Confirmed:
                    q = q.Where(a => a.IsConfirmed == true && a.IsCancelled != true);
                    break;
                case ArrivalStatusFilter.All:
                default:
                    q = q.Where(a => a.IsCancelled != true);
                    break;
            }

            // جستجو (ReportNumber, OrderNumber, Date, BiddingNumber, TotalPrice)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(a =>
                    (a.ReportNumber ?? "").Contains(search) ||
                    (a.BiddingNumber ?? "").Contains(search) ||
                    (a.OrderNumber ?? "").Contains(search));
            }

            q = q
                .OrderByDescending(a => a.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var list = await q
                .ToListAsync(token);

            return list.Select(a => new ArrivalListDto
            {
                ArrivalId = a.ArrivalId,
                DateString = a.Date?.ToString("yyyy/MM/dd"),
                ReportNumber = a.ReportNumber,
                BiddingNumber = a.BiddingNumber,
                TotalVolume = a.TotalVolume,
                IsConfirmed = a.IsConfirmed,
                IsDrafted = a.IsDrafted,
                IsCancelled = a.IsCancelled,
            }).ToList();
        }

        public async Task<ArrivalDto> GetById(int id, CancellationToken token)
        {
            var entity = await _ctx.TblArrivals
                .Include(a => a.TblArrivalLines)
                .FirstOrDefaultAsync(a => a.ArrivalId == id, token);

            if (entity == null) return null;

            var dto = new ArrivalDto
            {
                ArrivalId = entity.ArrivalId,
                DateString = entity.Date?.ToString("yyyy/MM/dd"),
                ReportNumber = entity.ReportNumber,
                BiddingNumber = entity.BiddingNumber,
                SourceId = entity.SourceId,
                FiscalPeriodId = entity.FiscalPeriodId,
                PackedVolume = entity.PackedVolume,
                TotalWeight = entity.TotalWeight,
                TotalVolume = entity.TotalVolume,
                Note = entity.Note,
                IsConfirmed = entity.IsConfirmed,
                IsDrafted = entity.IsDrafted,
                IsCancelled = entity.IsCancelled,
            };

            dto.Lines = entity.TblArrivalLines
                .Where(l => l.TotalVolume != null) // یا هر شرطی خواستی
                .Select(l => new ArrivalLineDto
                {
                    ArrivalLineId = l.ArrivalLineId,
                    ItemId = l.ItemId,
                    ManufacturerId = l.ManufacturerId,
                    StoreId = l.StoreId,
                    DosePerVialId = l.DosePerVialId,
                    NumVials = l.NumVials,
                    Quantity = l.Quantity,
                    UnitVolume = l.UnitVolume,
                    TotalVolume = l.TotalVolume,
                    BatchNum = l.BatchNum,
                    Note = l.Note,
                    ExpiryDateString = l.ExpiryDate?.ToString("yyyy/MM/dd"),
                    ProductionDateString = l.ProductionDate?.ToString("yyyy/MM/dd"),
                })
                .ToList();

            return dto;
        }

        public async Task<int> Save(ArrivalCreateEditDto dto, CancellationToken token)
        {
            TblArrival entity;

            bool isNew = !dto.ArrivalId.HasValue || dto.ArrivalId.Value == 0;

            if (isNew)
            {
                entity = new TblArrival
                {
                    EntryDate = DateTime.UtcNow,
                    IsCancelled = false,
                    IsConfirmed = false,
                    IsDrafted = true
                };
                _ctx.TblArrivals.Add(entity);
            }
            else
            {
                entity = await _ctx.TblArrivals
                    .Include(a => a.TblArrivalLines)
                    .FirstOrDefaultAsync(a => a.ArrivalId == dto.ArrivalId.Value, token);

                if (entity == null)
                    throw new Exception("Arrival not found");
            }

            // تبدیل DateString → DateTime (میلادی)
            if (!string.IsNullOrWhiteSpace(dto.DateString) &&
                DateTime.TryParse(dto.DateString, out var d))
            {
                entity.Date = d;
            }

            entity.ReportNumber = dto.ReportNumber;
            entity.BiddingNumber = dto.BiddingNumber;
            entity.SourceId = dto.SourceId;
            entity.FiscalPeriodId = dto.FiscalPeriodId;
            entity.PackedVolume = dto.PackedVolume;
            entity.TotalWeight = dto.TotalWeight;
            entity.TotalVolume = dto.TotalVolume;
            entity.Note = dto.Note;

            // وضعیت
            if (dto.Confirm)
            {
                entity.IsConfirmed = true;
                entity.IsDrafted = false;
                entity.ConfirmationTime = DateTime.UtcNow;
            }
            else if (dto.SaveAsDraft)
            {
                entity.IsDrafted = true;
                entity.IsConfirmed = false;
            }

            // لاین‌ها: مدل ساده — فعلاً همه را حذف و دوباره درج کن
            if (!isNew)
            {
                _ctx.TblArrivalLines.RemoveRange(entity.TblArrivalLines);
            }

            foreach (var line in dto.Lines)
            {
                DateTime? exp = null;
                if (!string.IsNullOrWhiteSpace(line.ExpiryDateString) &&
                    DateTime.TryParse(line.ExpiryDateString, out var e))
                    exp = e;

                DateTime? prod = null;
                if (!string.IsNullOrWhiteSpace(line.ProductionDateString) &&
                    DateTime.TryParse(line.ProductionDateString, out var p))
                    prod = p;

                entity.TblArrivalLines.Add(new TblArrivalLine
                {
                    ItemId = line.ItemId,
                    ManufacturerId = line.ManufacturerId,
                    StoreId = line.StoreId,
                    DosePerVialId = line.DosePerVialId,
                    NumVials = line.NumVials,
                    Quantity = line.Quantity,
                    UnitVolume = line.UnitVolume,
                    TotalVolume = line.TotalVolume,
                    BatchNum = line.BatchNum,
                    Note = line.Note,
                    ExpiryDate = exp,
                    ProductionDate = prod,
                    EntryDate = DateTime.UtcNow
                });
            }

            await _ctx.SaveChangesAsync(token);

            return entity.ArrivalId;
        }

        public async Task<bool> Delete(int id, CancellationToken token)
        {
            var entity = await _ctx.TblArrivals.FirstOrDefaultAsync(a => a.ArrivalId == id, token);
            if (entity == null) return false;

            entity.IsCancelled = true;
            await _ctx.SaveChangesAsync(token);
            return true;
        }

        public async Task<bool> DeleteLine(int lineId, CancellationToken token)
        {
            var line = await _ctx.TblArrivalLines.FirstOrDefaultAsync(l => l.ArrivalLineId == lineId, token);
            if (line == null) return false;

            _ctx.TblArrivalLines.Remove(line);
            await _ctx.SaveChangesAsync(token);
            return true;
        }
    }
}
