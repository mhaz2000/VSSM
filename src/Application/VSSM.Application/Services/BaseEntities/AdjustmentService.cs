using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Adjustments;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class AdjustmentService : IAdjustmentService
{
    private readonly VSSMDbContext _dbContext;
    private readonly IValidator<AdjustmentDto> _validator;

    public AdjustmentService(
        VSSMDbContext dbContext,
        IValidator<AdjustmentDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Create(
        AdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var fiscalPeriod = await _dbContext.TblFiscalPeriods.FirstOrDefaultAsync(c => c.IsActive == true);
        var itemDetailStore = await _dbContext.TblItemDetailStores.FirstOrDefaultAsync(c =>
            c.ItemDetailStoreId == dto.ItemDetailStoreId);

        var entity = new TblAdjustment
        {
            AdjustmentreasonId = dto.AdjustmentreasonId,
            FiscalPeriodId = fiscalPeriod is null ? -1 : fiscalPeriod.FiscalPeriodId,
            ArrivalLineId = -1,
            DispatchLineId = -1,
            ItemDetailStoreId = itemDetailStore is null ? -1 : itemDetailStore.ItemDetailStoreId,
            OldQuantity = itemDetailStore is null ? 0 : Convert.ToInt32(itemDetailStore.CurrentStock ?? 0),
            NewQuantity = dto.NewQuantity,
            EntryDate = DateTime.Now,
            Date = DateTime.Now,
            EnteredById = dto.EnteredById,
            Note = dto.Note,
        };

        await _dbContext.TblAdjustments.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.AdjustmentId;
    }

    public async Task<PagingSearchOrderDto<AdjustmentDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        int userId,
        CancellationToken cancellationToken)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var paging = new PagingModel
        {
            Page = page,
            PageSize = pageSize
        };

        var adjustmentsQuery = _dbContext.TblAdjustments
            .Where(a => a.EnteredById == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            adjustmentsQuery = adjustmentsQuery.Where(a => a.Note != null && a.Note.Contains(search));
        }

        paging.TotalItems = await adjustmentsQuery.CountAsync(cancellationToken);

        adjustmentsQuery = (sortField?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("date", "asc") => adjustmentsQuery.OrderBy(a => a.Date),
            ("date", "desc") => adjustmentsQuery.OrderByDescending(a => a.Date),
            ("entrydate", "asc") => adjustmentsQuery.OrderBy(a => a.EntryDate),
            ("entrydate", "desc") => adjustmentsQuery.OrderByDescending(a => a.EntryDate),
            _ => adjustmentsQuery.OrderByDescending(a => a.AdjustmentId)
        };
        
        var adjustments = await adjustmentsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var adjustreasonsId = adjustments.Select(c => c.AdjustmentreasonId);
        var adjustreasons = await _dbContext.TblAdjustreasons.ToListAsync(cancellationToken);
        adjustreasons = adjustreasons.Where(c => adjustreasonsId.Contains(c.AdjustreasonId)).ToList();

        var itemDetailStoreIds = adjustments.Select(a => a.ItemDetailStoreId).Distinct().ToList();
        var itemDetailStores = await _dbContext.TblItemDetailStores.ToListAsync(cancellationToken);
        itemDetailStores = itemDetailStores.Where(c => itemDetailStoreIds.Contains(c.ItemDetailStoreId)).ToList();

        var itemDetailIds = itemDetailStores.Select(ids => ids.ItemDetailId).Distinct().ToList();
        var itemDetails = await _dbContext.TblItemDetails.ToListAsync(cancellationToken);
        itemDetails = itemDetails.Where(c => itemDetailIds.Contains(c.ItemDetailId)).ToList();

        var itemIds = itemDetails.Select(id => id.ItemId).Distinct().ToList();
        var items = await _dbContext.TblItems.ToListAsync(cancellationToken);
        items = items.Where(c => itemIds.Contains(c.ItemId)).ToList();

        var itemClassIds = items.Select(i => i.ItemClassId).Distinct().ToList();
        var itemClasses = await _dbContext.TblItemsClasses.ToListAsync(cancellationToken);
        itemClasses = itemClasses.Where(c => itemClassIds.Contains(c.ItemClassId)).ToList();

        var itemDetailStoreDict = itemDetailStores.ToDictionary(x => x.ItemDetailStoreId);
        var itemDetailDict = itemDetails.ToDictionary(x => x.ItemDetailId);
        var itemDict = items.ToDictionary(x => x.ItemId);
        var itemClassDict = itemClasses.ToDictionary(x => x.ItemClassId);

        var adjustmentDtos = new List<AdjustmentDto>();

        foreach (var a in adjustments)
        {
            try
            {
                TblItemDetailStore? ids = null;
                TblItemDetail? id = null;
                TblItem? i = null;
                TblItemsClass? ic = null;

                if (a.ItemDetailStoreId != 0 && a.ItemDetailStoreId != null)
                    itemDetailStoreDict.TryGetValue(a.ItemDetailStoreId.Value, out ids);

                if (ids != null)
                    itemDetailDict.TryGetValue(ids.ItemDetailId.Value, out id);

                if (id != null)
                    itemDict.TryGetValue(id.ItemId.Value, out i);

                if (i != null)
                    itemClassDict.TryGetValue(i.ItemClassId.Value, out ic);

                var adjustreason = adjustreasons.FirstOrDefault(c => c.AdjustreasonId == a.AdjustmentreasonId);
                
                adjustmentDtos.Add(new AdjustmentDto
                {
                    AdjustmentId = a.AdjustmentId,
                    EntryDate = a.EntryDate,
                    Date = a.Date,
                    Note = a.Note,
                    AdjustmentreasonId = a.AdjustmentreasonId,
                    NewQuantity = a.NewQuantity,
                    OldQuantity = a.OldQuantity,
                    ArrivalLineId = a.ArrivalLineId,
                    DispatchLineId = a.DispatchLineId,
                    EnteredById = a.EnteredById,
                    FiscalPeriodId = a.FiscalPeriodId,
                    ItemDetailStoreId = a.ItemDetailStoreId,
                    ItemFullName = i?.FullName is null ? "" : i?.FullName,
                    ItemClassFullName = ic?.FullName is null ? "" : ic?.FullName,
                    AdjustreasonTitle = adjustreason?.FullName?? "" 
                });
            }
            catch (Exception e)
            {
                //ignore
            }
        }

        return new PagingSearchOrderDto<AdjustmentDto>
        {
            Items = adjustmentDtos,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }


    public async Task<AdjustmentDto?> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblAdjustments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.AdjustmentId == id,
                cancellationToken);

        if (entity is null)
            return null;

        return new AdjustmentDto
        {
            AdjustmentId = entity.AdjustmentId,
            EntryDate = entity.EntryDate,
            Date = entity.Date,
            Note = entity.Note,
            AdjustmentreasonId = entity.AdjustmentreasonId,
            NewQuantity = entity.NewQuantity,
            OldQuantity = entity.OldQuantity,
            ArrivalLineId = entity.ArrivalLineId,
            DispatchLineId = entity.DispatchLineId,
            EnteredById = entity.EnteredById,
            FiscalPeriodId = entity.FiscalPeriodId,
            ItemDetailStoreId = entity.ItemDetailStoreId
        };
    }

    public async Task<bool> Update(
        AdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = await _dbContext.TblAdjustments
            .FirstOrDefaultAsync(
                x => x.AdjustmentId == dto.AdjustmentId,
                cancellationToken);

        if (entity is null)
            return false;

        entity.Date = dto.Date;
        entity.Note = dto.Note;
        entity.AdjustmentreasonId = dto.AdjustmentreasonId;
        entity.NewQuantity = dto.NewQuantity;
        entity.OldQuantity = dto.OldQuantity;
        entity.ArrivalLineId = dto.ArrivalLineId;
        entity.DispatchLineId = dto.DispatchLineId;
        entity.EnteredById = dto.EnteredById;
        entity.FiscalPeriodId = dto.FiscalPeriodId;
        entity.ItemDetailStoreId = dto.ItemDetailStoreId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblAdjustments
            .FirstOrDefaultAsync(
                x => x.AdjustmentId == id,
                cancellationToken);

        if (entity is null)
            return false;

        _dbContext.TblAdjustments.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}