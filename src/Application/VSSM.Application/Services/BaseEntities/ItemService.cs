using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ItemService(VSSMDbContext dbContext, IValidator<ItemCreateDto> validator) : IItemService
{
    private const int VaccineClassId = 54;
    private const int DiluentClassId = 55;

    public async Task<int> Create(ItemCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var model = new TblItem
        {
            ItemId = dto.ItemId,
            FkClassId = dto.FkClassId,
            Code = dto.Code,
            EntryDate = DateTime.Now,
            FullName = dto.FullName,
            OrderId = dto.OrderId,
            IsActive = dto.IsActive,
            RequiresDiluent = dto.RequiresDiluent,
            DiluentId = dto.DiluentId,
            UniCode = dto.UniCode,
            Volume = dto.Volume,
            UserBelowZeroStore = dto.UserBelowZeroStore,
            UserAboveZeroStore = dto.UserAboveZeroStore,
            UseOrdinaryStore = dto.UseOrdinaryStore,
            UnitPrice = dto.UnitPrice,
            DayToWarmExpire = dto.DayToWarmExpire,
        };

        await dbContext.TblItems.AddAsync(model);
        await dbContext.SaveChangesAsync();
        return model.ItemId;
    }


    public async Task<List<ItemListDto>> GetActiveVaccines(CancellationToken cancellationToken)
    {
        var items = await dbContext.TblItems
            .AsNoTracking()
            .Where(c => c.IsActive == true && c.FkClassId == VaccineClassId)
            .ToListAsync(cancellationToken);

        return items.Select(x => new ItemListDto
        {
            ItemId = x.ItemId,
            FullName = x.FullName,
            FkClassId = x.FkClassId
        }).ToList();
    }

    public async Task<List<ItemListDto>> GetItemsByClassId(int fkClassId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TblItems
            .AsNoTracking()
            .Where(x => x.IsActive == true && x.FkClassId == fkClassId)
            .Select(x => new ItemListDto
            {
                ItemId = x.ItemId,
                FullName = x.FullName,
                FkClassId = x.FkClassId
            })
            .ToListAsync(cancellationToken);
    }




    public async Task<List<ItemListDto>> GetActives(CancellationToken cancellationToken)
    {
        var items = await dbContext.TblItems
            .AsNoTracking()
            .Where(c => c.IsActive == true)
            .ToListAsync(cancellationToken);

        return items.Select(x => new ItemListDto
        {
            ItemId = x.ItemId,
            FullName = x.FullName,
            FkClassId = x.FkClassId
        }).ToList();
    }

    public Task<List<ItemListDto>> GetAll(CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public async Task<ItemSelectDto> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblItems
            .AsNoTracking()
            .Where(x => x.ItemId == id)
            .FirstOrDefaultAsync(cancellationToken);

        return new ItemSelectDto
        {
            ItemId = id,
            ItemClassId = entity.FkClassId,
            Code = entity.Code,
            FullName = entity.FullName,
            IsActive = entity.IsActive,
            OrderId = entity.OrderId,   
            DayToWarmExpire = entity.DayToWarmExpire,
            DiluentId = entity.DiluentId,
            RequiresDiluent = entity.RequiresDiluent,
            UniCode = entity.UniCode,
            UnitPrice = entity.UnitPrice,
            UseOrdinaryStore = entity.UseOrdinaryStore,
            UserAboveZeroStore = entity.UserAboveZeroStore,
            UserBelowZeroStore = entity.UserBelowZeroStore,
            Volume = entity.Volume,
        };
    }

    public async Task<PagingSearchOrderDto<ItemListDto>> GetPagedList(
        string? search, int page, int pageSize,
        string sortField, string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };
        IQueryable<TblItem> query = dbContext.TblItems;

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.FullName.Contains(search) || x.Code.Contains(search));

        query = sortField switch
        {
            "Name" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            _ => sortOrder == "asc" ? query.OrderBy(x => x.OrderId) : query.OrderByDescending(x => x.OrderId)
        };

        paging.TotalItems = await query.CountAsync();

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(cancellationToken);

        var result = new List<ItemListDto>();
        foreach (var item in items)
        {
            var diluentItem = await dbContext.TblItems
                .FirstOrDefaultAsync(c => c.ItemId == item.DiluentId, cancellationToken);

            result.Add(new ItemListDto
            {
                ItemId = item.ItemId,
                FkClassId = item.FkClassId,
                FullName = item.FullName,
                Code = item.Code,
                UniCode = item.UniCode,
                OrderInList = item.OrderId.GetValueOrDefault(),
                IsActive = item.IsActive.GetValueOrDefault(),
                DiluentId = item.DiluentId,
                DiluentName = diluentItem?.FullName ?? "",
            });
        }

        return new PagingSearchOrderDto<ItemListDto>
        {
            Items = result,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<bool> Update(ItemCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var entity = await dbContext.TblItems
            .Where(x => x.ItemId == dto.ItemId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity != null)
        {
            entity.FkClassId = dto.FkClassId;
            entity.Code = dto.Code;
            entity.EntryDate = DateTime.Now;
            entity.FullName = dto.FullName;
            entity.OrderId = dto.OrderId;
            entity.IsActive = dto.IsActive;
            entity.RequiresDiluent = dto.RequiresDiluent;
            entity.DiluentId = dto.DiluentId;
            entity.UniCode = dto.UniCode;
            entity.Volume = dto.Volume;
            entity.UserBelowZeroStore = dto.UserBelowZeroStore;
            entity.UserAboveZeroStore = dto.UserAboveZeroStore;
            entity.UseOrdinaryStore = dto.UseOrdinaryStore;
            entity.UnitPrice = dto.UnitPrice;
            entity.DayToWarmExpire = dto.DayToWarmExpire;
        }

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<ItemListDto>> GetItemsByClassId(int classItemId)
    {
        return await dbContext.TblItems
            .AsNoTracking()
            .Where(item => item.FkClassId == classItemId)
            .Select(item => new ItemListDto
            {
                ItemId = item.ItemId,
                FkClassId = item.FkClassId,
                FullName = item.FullName,
                Code = item.Code,
                UniCode = item.UniCode,
                OrderInList = item.OrderId.GetValueOrDefault(),
                IsActive = item.IsActive.GetValueOrDefault(),
                DiluentId = item.DiluentId,
                DiluentName = string.Empty
            })
            .ToListAsync();
    }

    public async Task<string> GetItemNamesAsync(List<int> itemIds)
    {
        var items = await dbContext.TblItems
            .Where(t => itemIds.Contains(t.ItemId))
            .Select(s => s.FullName)
            .ToListAsync();

        return string.Join(" - ", items);
    }
}