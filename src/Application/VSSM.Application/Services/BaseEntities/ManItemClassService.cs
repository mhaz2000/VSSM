using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ManItemClassService(
    VSSMDbContext dbContext,
    IBaseInfoService baseInfoService,
    IValidator<ManItemClassDto> validator) : IManItemClassService
{
    public async Task<long> Create(ManItemClassDto dto, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = new TblManItemClass
        {
            ItemClassId = dto.ItemClassCode,
            ManufacturerId = dto.ManufacturerId   // Auto-selected = first of list
        };

        await dbContext.TblManItemClasses.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ManItemClassId;
    }


    public async Task<PagingSearchOrderDto<ManItemClassDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblManItemClass> query = dbContext.TblManItemClasses.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.ItemClassId.HasValue && x.ItemClassId.Value.ToString().Contains(search)) ||
                (x.ManufacturerId.HasValue && x.ManufacturerId.Value.ToString().Contains(search)) ||
                x.ManItemClassId.ToString().Contains(search)
            );
        }

        // Sorting
        query = sortField switch
        {
            "ManufacturerId" => sortOrder == "asc"
                ? query.OrderBy(x => x.ManufacturerId)
                : query.OrderByDescending(x => x.ManufacturerId),

            _ => sortOrder == "asc"
                ? query.OrderBy(x => x.ManItemClassId)
                : query.OrderByDescending(x => x.ManItemClassId)
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new ManItemClassDto
            {
                ManItemClassId = x.ManItemClassId,
                ItemClassCode = x.ItemClassId,

                // Convert the single ManufacturerId → List<int>
                ManufacturerIds = x.ManufacturerId != null
                    ? new List<int> { x.ManufacturerId.Value }
                    : new List<int>()
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<ManItemClassDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }


    public async Task<ManItemClassDto?> GetById(long id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblManItemClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ManItemClassId == id, cancellationToken);

        if (entity == null)
            return null;

        return new ManItemClassDto
        {
            ManItemClassId = entity.ManItemClassId,
            ItemClassCode = entity.ItemClassId,

            // Convert one → list
            ManufacturerIds = entity.ManufacturerId != null
                ? new List<int> { entity.ManufacturerId.Value }
                : new List<int>()
        };
    }


    public async Task<List<ManufacturerDropDownDto>> ManufacturerDropDown(
        CancellationToken cancellationToken)
    {
        var entity = await baseInfoService.GetByParentIdAsync(
            48,
            cancellationToken);

        return entity.Select(c => new ManufacturerDropDownDto
        {
            ItemsClassId = c.Id,
            Name = c.Title
        }).ToList();
    }


    public async Task<bool> Update(ManItemClassDto dto, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var entity = await dbContext.TblManItemClasses
            .FirstOrDefaultAsync(x => x.ManItemClassId == dto.ManItemClassId, cancellationToken);

        if (entity == null)
            return false;

        entity.ItemClassId = dto.ItemClassCode;
        entity.ManufacturerId = dto.ManufacturerId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }


    public async Task<bool> Delete(long id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblManItemClasses
            .FirstOrDefaultAsync(x => x.ManItemClassId == id, cancellationToken);

        if (entity == null)
            return false;

        dbContext.TblManItemClasses.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
