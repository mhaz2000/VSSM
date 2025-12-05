using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.DTOs.BaseEntities.StoreType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class StoreTypeService : IStoreTypeService
{
    private readonly VSSMDbContext _dbContext;
    private readonly IValidator<StoreTypeDto> _validator;

    public StoreTypeService(VSSMDbContext dbContext, IValidator<StoreTypeDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Create(StoreTypeDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = new TblStoreType
        {
            Code = dto.Code,
            FullName = dto.FullName,
            EntryDate = dto.EntryDate ?? DateTime.Now,
            IsActive = dto.IsActive,
            IsUserEditable = dto.IsUserEditable
        };

        await _dbContext.TblStoreTypes.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.StoreTypeId;
    }

    public async Task<PagingSearchOrderDto<StoreTypeDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblStoreType> query = _dbContext.TblStoreTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.FullName.Contains(search) ||
                x.StoreTypeId.ToString().Contains(search)
            );
        }

        query = sortField switch
        {
            "Code" => sortOrder == "asc"
                ? query.OrderBy(x => x.Code)
                : query.OrderByDescending(x => x.Code),
            "FullName" => sortOrder == "asc"
                ? query.OrderBy(x => x.FullName)
                : query.OrderByDescending(x => x.FullName),
            "EntryDate" => sortOrder == "asc"
                ? query.OrderBy(x => x.EntryDate)
                : query.OrderByDescending(x => x.EntryDate),
            "IsActive" => sortOrder == "asc"
                ? query.OrderBy(x => x.IsActive)
                : query.OrderByDescending(x => x.IsActive),
            _ => sortOrder == "asc"
                ? query.OrderBy(x => x.StoreTypeId)
                : query.OrderByDescending(x => x.StoreTypeId),
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new StoreTypeDto
            {
                StoreTypeId = x.StoreTypeId,
                Code = x.Code,
                FullName = x.FullName,
                EntryDate = x.EntryDate,
                IsActive = x.IsActive ?? false,
                IsUserEditable = x.IsUserEditable ?? false
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<StoreTypeDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<StoreTypeDto> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblStoreTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StoreTypeId == id, cancellationToken);

        if (entity == null)
            return null;

        return new StoreTypeDto
        {
            StoreTypeId = entity.StoreTypeId,
            Code = entity.Code,
            FullName = entity.FullName,
            EntryDate = entity.EntryDate,
            IsActive = entity.IsActive ?? false,
            IsUserEditable = entity.IsUserEditable ?? false
        };
    }

    public async Task<bool> Update(StoreTypeDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = await _dbContext.TblStoreTypes
            .FirstOrDefaultAsync(x => x.StoreTypeId == dto.StoreTypeId, cancellationToken);

        if (entity == null)
            return false;

        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.EntryDate = dto.EntryDate;
        entity.IsActive = dto.IsActive;
        entity.IsUserEditable = dto.IsUserEditable;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblStoreTypes
            .FirstOrDefaultAsync(x => x.StoreTypeId == id, cancellationToken);

        if (entity == null)
            return false;

        _dbContext.TblStoreTypes.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<StoreTypeDto>> GetAll(CancellationToken cancellationToken)
    {

        IQueryable<TblStoreType> query = _dbContext.TblStoreTypes;
        return await query.Select(x =>
               new StoreTypeDto
               {
                   StoreTypeId = x.StoreTypeId,
                   Code = x.Code,
                   FullName = x.FullName,
                   EntryDate = x.EntryDate,
                   IsActive = x.IsActive ?? false,
                   IsUserEditable = x.IsUserEditable ?? false
               }
            ).ToListAsync(cancellationToken);
    }
}