using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class StoreService(VSSMDbContext db) : IStoreService
{
    public async Task<StoreEditDto?> GetById(int id, CancellationToken token)
    {
        var entity = await db.TblStores
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, token);

        if (entity == null)
            return null;

        return new StoreEditDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            TypeId = entity.TypeId ?? 0,
            FreeVolume = entity.FreeVolume ?? 0,
            Volume = entity.Volume,
            Note = entity.Note,
            IsActive = entity.IsActive ?? false,
            RecipientId = entity.RecipientId,
            Year = entity.Year,
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
            IsActive = true,
            RecipientId = dto.RecipientId,
            Year = dto.Year,
        };

        await db.TblStores.AddAsync(entity, token);
        await db.SaveChangesAsync(token);

        return entity.Id;
    }

    public async Task<bool> Update(StoreEditDto dto, CancellationToken token)
    {
        var entity = await db.TblStores
            .FirstOrDefaultAsync(x => x.Id == dto.Id, token);

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
        entity.Year = dto.Year;

        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> ToggleActive(int id, CancellationToken token)
    {
        var entity = await db.TblStores
            .FirstOrDefaultAsync(x => x.Id == id, token);

        if (entity == null)
            throw new Exception("Record not found.");

        entity.IsActive = !entity.IsActive;
        await db.SaveChangesAsync(token);

        return entity.IsActive ?? false;
    }

    public async Task<PagingSearchOrderDto<StoreListDto>> GetPagedList(
        string? search,
        string? recipientId,
        int? typeId,
        int page,
        int pageSize,
        CancellationToken token)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        var query = db.TblStores.AsQueryable();

        if (typeId.HasValue && typeId.Value > 0)
            query = query.Where(x => x.TypeId == typeId);

        if (!string.IsNullOrEmpty(recipientId) && int.TryParse(recipientId, out var recId))
            query = query.Where(x => x.RecipientId == recId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search));
        }

        paging.TotalItems = await query.CountAsync(token);

        var items = await query
            .OrderBy(x => x.Id)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new StoreListDto
            {
                StoreId = x.Id,

                RecipientId = x.RecipientId,

                RecipientName = db.TblRecipients
                    .Where(r => r.RecipientId == x.RecipientId)
                    .Select(r => r.FullName)
                    .FirstOrDefault() ?? string.Empty,

                Code = x.Code ?? string.Empty,
                FullName = x.Name ?? string.Empty,
                TypeId = x.TypeId ?? 0,
                TypeName = db.TblBaseInfo
                    .Where(t => t.BaseInfoId == x.TypeId)
                    .Select(t => t.Title)
                    .FirstOrDefault() ?? string.Empty,
                Volume = x.Volume,
                FreeVolume = x.FreeVolume,
                Note = x.Note ?? string.Empty,
                IsActive = x.IsActive ?? false,
                Year = x.Year
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
        var query = db.TblStores.Where(z => z.IsActive.Value);
        return await query.Select(x => new StoreListDto
        {
            RecipientId = x.RecipientId,
            StoreId = x.Id,
            Code = x.Code,
            TypeId = x.TypeId ?? 0,
            FullName = x.Name,
            FreeVolume = x.FreeVolume ?? 0,
            IsActive = x.IsActive ?? false
        }).ToListAsync(cancellationToken);
    }

    public async Task<List<StoreListDto>> GetAllWithRecipientId(int recipientId, CancellationToken cancellationToken)
    {
        var query = db.TblStores.Where(z => z.RecipientId!.Value == recipientId && z.IsActive!.Value);
        return await query.Select(x => new StoreListDto
        {
            RecipientId = x.RecipientId,
            StoreId = x.Id,
            Code = x.Code,
            TypeId = x.TypeId ?? 0,
            FullName = x.Name,
            FreeVolume = x.FreeVolume ?? 0,
            IsActive = x.IsActive ?? false
        }).ToListAsync(cancellationToken);
    }

    public async Task<(List<StoreEquipmentReportDto> data, int count)> GetStoreEquipmentReport(
        int currentRecipientId,
        int? recipientId,
        int? storeTypeId,
        int? volumeStart,
        int? volumeEnd,
        int? freeVolumeStart,
        int? freeVolumeEnd,
        int? yearStart,
        int? yearEnd,
        int? isActive,
        int pageNumber,
        int pageSize,
        string sort = "Recipient",
        string direction = "asc",
        CancellationToken cancellationToken = default)
    {
        var paging = new PagingModel { Page = pageNumber, PageSize = pageSize };
        var recipients = recipientId is null
            ? db.TblRecipients.Where(t => t.RecipientId == currentRecipientId)
            : db.TblRecipients.Where(t => t.RecipientId == recipientId);

        var stores = db.TblStores.AsQueryable().AsNoTracking();

        if (storeTypeId.HasValue && storeTypeId > 0)
            stores = stores.Where(x => x.TypeId == storeTypeId.Value);

        if (isActive.HasValue && isActive > -1)
        {
            var active = isActive.Value == 1;
            stores = stores.Where(x => x.IsActive == active);
        }

        if (volumeStart.HasValue && volumeStart > 0)
            stores = stores.Where(x => x.Volume >= volumeStart.Value);

        if (volumeEnd.HasValue && volumeEnd > 0)
            stores = stores.Where(x => x.Volume <= volumeEnd.Value);

        if (freeVolumeStart.HasValue && freeVolumeStart > 0)
            stores = stores.Where(x => x.FreeVolume >= freeVolumeStart.Value);

        if (freeVolumeEnd.HasValue && freeVolumeEnd > 0)
            stores = stores.Where(x => x.FreeVolume <= freeVolumeEnd.Value);

        if (yearStart.HasValue && yearStart > 0)
            stores = stores.Where(x => x.Year >= yearStart.Value);

        if (yearEnd.HasValue && yearEnd > 0)
            stores = stores.Where(x => x.Year <= yearEnd.Value);

        var query = from r in recipients
                    join s in stores on r.RecipientId equals s.RecipientId
                    join t in db.TblBaseInfo on s.TypeId equals t.BaseInfoId
                    select new StoreEquipmentReportDto
                    {
                        StoreName = s.Name,
                        StoreType = t.Title,
                        Capacity = s.Volume,
                        TotalCapacity = s.FreeVolume,
                        IsActive = s.IsActive.Value ? "فعال" : "غیر فعال",
                        Year = s.Year,
                    };

        var total = await query.CountAsync(cancellationToken);
        query = query.ApplySorting(sort, direction == "desc");

        return (
            await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken),
            total
        );
    }

    public async Task<string> GetStoreNamesAsync(List<int> storeIds)
    {
        var stores = await db.TblStores
            .Where(t => storeIds.Contains(t.Id))
            .Select(s => s.Name)
            .ToListAsync();

        return string.Join(" - ", stores);
    }
}