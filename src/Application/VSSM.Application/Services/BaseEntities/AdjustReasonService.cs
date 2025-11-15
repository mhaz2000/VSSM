using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.AdjustReason;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class AdjustReasonService : IAdjustReasonService
{
    private readonly VSSMDbContext _dbContext;
    private readonly IValidator<AdjustreasonDto> _validator;

    public AdjustReasonService(VSSMDbContext dbContext, IValidator<AdjustreasonDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    // Create
    public async Task<int> Create(AdjustreasonDto dto, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var maxId = await _dbContext.TblAdjustreasons
            .MaxAsync(x => (int?)x.AdjustreasonId, cancellationToken) ?? 0;

        var entity = new TblAdjustreason
        {
            Code = dto.Code,
            FullName = dto.FullName,
            EntryDate = dto.EntryDate,
            IsActive = dto.IsActive,
            UsedForDispatchAdjust = dto.UsedForDispatchAdjust,
            UserForArrivalAdjust = dto.UserForArrivalAdjust,
            UsedForStockAdjust = dto.UsedForStockAdjust,
            IsWaste = dto.IsWaste,
            ApplicationForStockSubtraction = dto.ApplicationForStockSubtraction,
            ApplicationForStockAddition = dto.ApplicationForStockAddition
        };

        await _dbContext.TblAdjustreasons.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.AdjustreasonId;
    }

    // Get Paged List
    public async Task<PagingSearchOrderDto<AdjustreasonDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblAdjustreason> query = _dbContext.TblAdjustreasons.AsNoTracking();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.Code.Contains(search) ||
                x.AdjustreasonId.ToString().Contains(search));
        }

        // SORTING
        query = sortField switch
        {
            "Code" => sortOrder == "asc" ?
                query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),

            "FullName" => sortOrder == "asc" ?
                query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),

            "EntryDate" => sortOrder == "asc" ?
                query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),

            _ => sortOrder == "asc" ?
                query.OrderBy(x => x.AdjustreasonId) : query.OrderByDescending(x => x.AdjustreasonId)
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new AdjustreasonDto
            {
                AdjustReasonId = x.AdjustreasonId,
                Code = x.Code,
                FullName = x.FullName,
                EntryDate = x.EntryDate,
                IsActive = x.IsActive ?? false,
                UsedForDispatchAdjust = x.UsedForDispatchAdjust ?? false,
                UserForArrivalAdjust = x.UserForArrivalAdjust ?? false,
                UsedForStockAdjust = x.UsedForStockAdjust ?? false,
                IsWaste = x.IsWaste ?? false,
                ApplicationForStockSubtraction = x.ApplicationForStockSubtraction ?? false,
                ApplicationForStockAddition = x.ApplicationForStockAddition ?? false
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<AdjustreasonDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    // Get By ID
    public async Task<AdjustreasonDto?> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblAdjustreasons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AdjustreasonId == id, cancellationToken);

        if (entity == null)
            return null;

        return new AdjustreasonDto
        {
            AdjustReasonId = entity.AdjustreasonId,
            Code = entity.Code,
            FullName = entity.FullName,
            EntryDate = entity.EntryDate,
            IsActive = entity.IsActive  ?? false,
            UsedForDispatchAdjust = entity.UsedForDispatchAdjust ?? false,
            UserForArrivalAdjust = entity.UserForArrivalAdjust ?? false,
            UsedForStockAdjust = entity.UsedForStockAdjust ?? false,
            IsWaste = entity.IsWaste ?? false,
            ApplicationForStockSubtraction = entity.ApplicationForStockSubtraction ?? false,
            ApplicationForStockAddition = entity.ApplicationForStockAddition ?? false
        };
    }

    // Update
    public async Task<bool> Update(AdjustreasonDto dto, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await _dbContext.TblAdjustreasons
            .FirstOrDefaultAsync(x => x.AdjustreasonId == dto.AdjustReasonId, cancellationToken);

        if (entity == null)
            return false;

        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.EntryDate = dto.EntryDate;
        entity.IsActive = dto.IsActive;
        entity.UsedForDispatchAdjust = dto.UsedForDispatchAdjust;
        entity.UserForArrivalAdjust = dto.UserForArrivalAdjust;
        entity.UsedForStockAdjust = dto.UsedForStockAdjust;
        entity.IsWaste = dto.IsWaste;
        entity.ApplicationForStockSubtraction = dto.ApplicationForStockSubtraction;
        entity.ApplicationForStockAddition = dto.ApplicationForStockAddition;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Delete
    public async Task<bool> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblAdjustreasons
            .FirstOrDefaultAsync(x => x.AdjustreasonId == id, cancellationToken);

        if (entity == null)
            return false;

        _dbContext.TblAdjustreasons.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}