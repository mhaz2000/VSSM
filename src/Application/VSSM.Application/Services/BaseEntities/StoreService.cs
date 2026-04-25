using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Reports;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Entities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class StoreService : IStoreService
    {
        private readonly VSSMDbContext _db;

        public StoreService(VSSMDbContext db)
        {
            _db = db;
        }

        public async Task<StoreEditDto> GetById(int id, CancellationToken token)
        {
            var entity = await _db.TblStores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StoreId == id, token);

            if (entity == null)
                throw new Exception("Record not found.");

            return new StoreEditDto
            {
                StoreId = entity.StoreId,
                Code = entity.Code,
                Name = entity.Name,
                TypeId = entity.TypeId ?? 0,
                FreeVolume = entity.FreeVolume ?? 0,
                Volume = entity.Volume,
                Note = entity.Note,
                IsActive = entity.IsActive ?? false,
                RecipientId = entity.RecipientId,
                GrossVol = entity.GrossVol,
                Year = entity.Year,
                Model = entity.Model,
                Manufacturer = entity.Manufacturer,
                Pqsno = entity.Pqsno,
                CoolingUnitNo = entity.CoolingUnitNo,
                RefrigerantGas = entity.RefrigerantGas,
                RoomBox = entity.RoomBox,
                PowerSource = entity.PoweSource,
                Source = entity.Source,
                Barcode = entity.Barcode,
                PeriodNoteUse = entity.PeriodNotUse,
                UnitCost = entity.UnitCost
            };
        }

        public async Task<int> Create(StoreCreateDto dto, CancellationToken token)
        {
            var entity = new TblStore
            {
                Code = dto.Code,
                Name = dto.Name,
                TypeId = dto.TypeId,
                FreeVolume = dto.FreeVolume,
                Volume = dto.Volume,
                Note = dto.Note,
                IsActive = dto.IsActive,
                RecipientId = dto.RecipientId,
                GrossVol = dto.GrossVol,
                Year = dto.Year,
                Model = dto.Model,
                Manufacturer = dto.Manufacturer,
                Pqsno = dto.Pqsno,
                CoolingUnitNo = dto.CoolingUnitNo,
                RefrigerantGas = dto.RefrigerantGas,
                RoomBox = dto.RoomBox,
                PoweSource = dto.PowerSource,
                Source = dto.Source,
                Barcode = dto.Barcode,
                PeriodNotUse = dto.PeriodNoteUse,
                UnitCost = dto.UnitCost,
                EntryDate = DateTime.Now
            };

            _db.TblStores.Add(entity);
            await _db.SaveChangesAsync(token);

            return entity.StoreId;
        }

        public async Task<bool> Update(StoreEditDto dto, CancellationToken token)
        {
            var entity = await _db.TblStores
                .FirstOrDefaultAsync(x => x.StoreId == dto.StoreId, token);

            if (entity == null)
                throw new Exception("Record not found.");

            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.TypeId = dto.TypeId;
            entity.FreeVolume = dto.FreeVolume;
            entity.Volume = dto.Volume;
            entity.Note = dto.Note;
            entity.IsActive = dto.IsActive;
            entity.RecipientId = dto.RecipientId;
            entity.GrossVol = dto.GrossVol;
            entity.Year = dto.Year;
            entity.Model = dto.Model;
            entity.Manufacturer = dto.Manufacturer;
            entity.Pqsno = dto.Pqsno;
            entity.CoolingUnitNo = dto.CoolingUnitNo;
            entity.RefrigerantGas = dto.RefrigerantGas;
            entity.RoomBox = dto.RoomBox;
            entity.PoweSource = dto.PowerSource;
            entity.Source = dto.Source;
            entity.Barcode = dto.Barcode;
            entity.PeriodNotUse = dto.PeriodNoteUse;
            entity.UnitCost = dto.UnitCost;

            await _db.SaveChangesAsync(token);
            return true;
        }

        public async Task<PagingSearchOrderDto<StoreListDto>> GetPagedList(
       string? search,
       int? typeId,
       int page,
       int pageSize,
       CancellationToken token)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            var query = _db.TblStores.AsQueryable();

            // ---------------- FILTER: TypeId ----------------
            if (typeId.HasValue && typeId.Value > 0)
                query = query.Where(x => x.TypeId == typeId);

            // ---------------- SEARCH: Code + FullName ----------------
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    x.Code.Contains(search) ||
                    x.Name.Contains(search));
            }

            paging.TotalItems = await query.CountAsync(token);

            var items = await query
                .OrderBy(x => x.StoreId)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new StoreListDto
                {
                    StoreId = x.StoreId,
                    Code = x.Code,
                    FullName = x.Name,
                    TypeId = x.TypeId ?? 0,

                    TypeName = _db.TblStoreTypes
                        .Where(t => t.StoreTypeId == x.TypeId)
                        .Select(t => t.FullName)
                        .FirstOrDefault(),

                    Volume = x.Volume,
                    FreeVolume = x.FreeVolume,
                    IsActive = x.IsActive ?? false,
                    Note = string.IsNullOrEmpty(x.Note) ? string.Empty : x.Note
                })
                .ToListAsync(token);

            return new PagingSearchOrderDto<StoreListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search
            };
        }


        public async Task<List<StoreListDto>> GetAll(CancellationToken cancellationToken)
        {

            IQueryable<TblStore> query = _db.TblStores.Where(z => z.IsActive.Value);
            return await query.Select(x =>
                   new StoreListDto
                   {
                       RecipientId = x.RecipientId,
                       StoreId = x.StoreId,
                       Code = x.Code,
                       TypeId= x.TypeId ?? 0,
                       FullName = x.Name,
                       FreeVolume = x.FreeVolume ?? 0,
                       IsActive = x.IsActive ?? false
                   }
                ).ToListAsync(cancellationToken);
        }
        
        public async Task<List<StoreListDto>> GetAllWithRecipientId(int recipientId,CancellationToken cancellationToken)
        {
            IQueryable<TblStore> query = _db.TblStores.Where(z => z.RecipientId!.Value == recipientId && z.IsActive!.Value);
            return await query.Select(x =>
                new StoreListDto
                {
                    RecipientId = x.RecipientId,
                    StoreId = x.StoreId,
                    Code = x.Code,
                    TypeId= x.TypeId ?? 0,
                    FullName = x.Name,
                    FreeVolume = x.FreeVolume ?? 0,
                    IsActive = x.IsActive ?? false
                }
            ).ToListAsync(cancellationToken);
        }


        public async Task<(List<StoreEquipmentReportDto> data, int count)> GetStoreEquipmentReport(int currentRecipientId,
            int? recipientId,
            int? storeTypeId,
            int? volumeStart,
            int? volumeEnd,
            int? freeVolumeStart,
            int? freeVolumeEnd,
            int? yearStart,
            int? yearEnd,
            bool isActive,
            int pageNumber,
            int pageSize,
            string sort = "Recipient",
            string direction = "asc",
            CancellationToken cancellationToken = default)
        {
            var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };
            var reciepients = recipientId is null ? _db.TblRecipients.Where(t => t.RecipientId == currentRecipientId) : _db.TblRecipients.Where(t => t.RecipientId == recipientId);
            
            var stores = _db.TblStores.AsQueryable();

            if (storeTypeId.HasValue && storeTypeId > 0)
            {
                stores = stores.Where(x => x.TypeId == storeTypeId.Value);
            }


            if (volumeStart.HasValue && volumeStart > 0)
            {
                stores = stores.Where(x => x.Volume >= volumeStart.Value);
            }
            if (volumeEnd.HasValue && volumeEnd > 0)
            {
                stores = stores.Where(x => x.Volume <= volumeEnd.Value);
            }


            if (freeVolumeStart.HasValue && freeVolumeStart > 0)
            {
                stores = stores.Where(x => x.FreeVolume >= freeVolumeStart.Value);
            }
            if (freeVolumeEnd.HasValue && freeVolumeEnd > 0)
            {
                stores = stores.Where(x => x.FreeVolume <= freeVolumeEnd.Value);
            }


            if (yearStart.HasValue && yearStart > 0)
            {
                stores = stores.Where(x => x.Year >= yearStart.Value);
            }
            if (yearEnd.HasValue && yearEnd > 0)
            {
                stores = stores.Where(x => x.Year <= yearEnd.Value);
            }


            var query = from r in reciepients
                        join s in stores on r.RecipientId equals s.RecipientId
                        join t in _db.TblStoreTypes on s.TypeId equals t.StoreTypeId
                        select new StoreEquipmentReportDto()
                        {
                            StoreName = s.Name,
                            StoreType = t.FullName,
                            Capacity = s.Volume,
                            TotalCapacity = s.FreeVolume,
                            IsActive = s.IsActive,
                            Year = s.Year,
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
