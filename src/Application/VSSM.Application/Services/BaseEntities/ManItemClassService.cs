using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ManItemClassService : IManItemClassService
{
    private readonly VSSMDbContext _dbContext;
    private readonly IValidator<ManItemClassDto> _validator;

    public ManItemClassService(VSSMDbContext dbContext, IValidator<ManItemClassDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<long> Create(ManItemClassDto dto, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var entity = new TblManItemClass
        {
            ItemClassId = dto.ItemClassCode,
            ManufacturerId = dto.ManufacturerId
        };

        await _dbContext.TblManItemClasses.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
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

        IQueryable<TblManItemClass> query = _dbContext.TblManItemClasses.AsNoTracking();

        // Search on ItemClassCode, ManufacturerId, ManItemClassId
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
                ManufacturerId = x.ManufacturerId
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
        var entity = await _dbContext.TblManItemClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ManItemClassId == id, cancellationToken);

        if (entity == null)
            return null;

        return new ManItemClassDto
        {
            ManItemClassId = entity.ManItemClassId,
            ItemClassCode = entity.ItemClassId,
            ManufacturerId = entity.ManufacturerId
        };
    }
    
    public async Task<List<ManufacturerDropDownDto>> ManufacturerDropDown(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblManufacturers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (entity == null)
            return null;

        return entity.Select(c => new ManufacturerDropDownDto()
        {
            ManufacturerId = c.ManufacturerId,
            Name = c.FullName
        }).ToList();
    }


    // ---------------- UPDATE ----------------
    public async Task<bool> Update(ManItemClassDto dto, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var entity = await _dbContext.TblManItemClasses
            .FirstOrDefaultAsync(x => x.ManItemClassId == dto.ManItemClassId, cancellationToken);

        if (entity == null) return false;

        entity.ItemClassId = dto.ItemClassCode;
        entity.ManufacturerId = dto.ManufacturerId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ---------------- DELETE ----------------
    public async Task<bool> Delete(long id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblManItemClasses
            .FirstOrDefaultAsync(x => x.ManItemClassId == id, cancellationToken);

        if (entity == null) return false;

        _dbContext.TblManItemClasses.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}