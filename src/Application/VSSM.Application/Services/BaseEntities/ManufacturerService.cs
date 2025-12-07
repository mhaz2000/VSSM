using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ManItemClass;
using VSSM.Application.DTOs.BaseEntities.Manufacturer;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ManufacturerService : IManufacturerService
{
    private readonly VSSMDbContext _dbContext;
    private readonly IValidator<ManufacturerDto> _validator;

    public ManufacturerService(VSSMDbContext dbContext, IValidator<ManufacturerDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    // --- Create ---
    public async Task<int> Create(ManufacturerDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);


        var entity = new TblManufacturer
        {
            Code = dto.Code,
            FullName = dto.FullName,
            EntryDate = dto.EntryDate,
            ShowForVac = dto.ShowForVac,
            ShowForOther = dto.ShowForOther,
            IsActive = dto.IsActive
        };

        await _dbContext.TblManufacturers.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var itemClasses = dto.ItemClasses.Select(c => new TblManItemClass()
        {
            ManufacturerId = entity.ManufacturerId,
            ItemClassId = c
        });

        _dbContext.TblManItemClasses.AddRange(itemClasses);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.ManufacturerId;
    }

    public async Task<PagingSearchOrderDto<ManufacturerDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblManufacturer> query = _dbContext.TblManufacturers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.Code != null && x.Code.Contains(search)) ||
                (x.FullName != null && x.FullName.Contains(search)) ||
                x.ManufacturerId.ToString().Contains(search)
            );
        }

        query = sortField switch
        {
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            "EntryDate" => sortOrder == "asc" ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),
            "ShowForVac" => sortOrder == "asc" ? query.OrderBy(x => x.ShowForVac) : query.OrderByDescending(x => x.ShowForVac),
            "ShowForOther" => sortOrder == "asc" ? query.OrderBy(x => x.ShowForOther) : query.OrderByDescending(x => x.ShowForOther),
            "IsActive" => sortOrder == "asc" ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive),
            _ => sortOrder == "asc" ? query.OrderBy(x => x.ManufacturerId) : query.OrderByDescending(x => x.ManufacturerId),
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new ManufacturerDto
            {
                ManufacturerId = x.ManufacturerId,
                Code = x.Code,
                FullName = x.FullName,
                EntryDate = x.EntryDate,
                ShowForVac = x.ShowForVac ?? false,
                ShowForOther = x.ShowForOther ?? false,
                IsActive = x.IsActive ?? false
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<ManufacturerDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<List<ManufacturerDropDownDto>> ManufacturerDropDown(long? id, CancellationToken cancellationToken)
    {
        var allItemClasses = await _dbContext.TblItemsClasses
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        List<TblManItemClass> thisIdClasses = new List<TblManItemClass>();
        if (id is not null)
        {
            thisIdClasses = await _dbContext.TblManItemClasses
                .Where(c => c.ManufacturerId == id)
                .AsNoTracking()
                .ToListAsync(cancellationToken);


        }

        List<ManufacturerDropDownDto> result = new();
        foreach (var itemClass in allItemClasses)
        {
            var isExist = thisIdClasses.FirstOrDefault(c => c.ItemClassId == itemClass.ItemClassId);
            result.Add(new ManufacturerDropDownDto
            {
                ItemsClassId = itemClass.ItemClassId,
                Name = itemClass.FullName,
                Selected = isExist is not null
            });
        }

        return result;
    }

    public async Task<ManufacturerDto> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblManufacturers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ManufacturerId == id, cancellationToken);

        if (entity == null)
            return null;

        return new ManufacturerDto
        {
            ManufacturerId = entity.ManufacturerId,
            Code = entity.Code,
            FullName = entity.FullName,
            EntryDate = entity.EntryDate,
            ShowForVac = entity.ShowForVac ?? false,
            ShowForOther = entity.ShowForOther ?? false,
            IsActive = entity.IsActive ?? false
        };
    }

    public async Task<bool> Update(ManufacturerDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = await _dbContext.TblManufacturers
            .FirstOrDefaultAsync(x => x.ManufacturerId == dto.ManufacturerId, cancellationToken);

        if (entity == null)
            return false;

        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.EntryDate = dto.EntryDate;
        entity.ShowForVac = dto.ShowForVac;
        entity.ShowForOther = dto.ShowForOther;
        entity.IsActive = dto.IsActive;

        var allThisItemClasses =
            await _dbContext.TblManItemClasses.Where(c => c.ManufacturerId == dto.ManufacturerId).ToListAsync();

        _dbContext.TblManItemClasses.RemoveRange(allThisItemClasses);

        var itemClasses = dto.ItemClasses.Select(c => new TblManItemClass()
        {
            ManufacturerId = entity.ManufacturerId,
            ItemClassId = c
        });

        _dbContext.TblManItemClasses.AddRange(itemClasses);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblManufacturers
            .FirstOrDefaultAsync(x => x.ManufacturerId == id, cancellationToken);

        if (entity == null)
            return false;

        _dbContext.TblManufacturers.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<ManufacturerDto>> GetActives(CancellationToken cancellationToken)
    {
        var items = await _dbContext.TblManufacturers
                                .AsNoTracking()
                                .Where(c => c.IsActive == true)
                                .ToListAsync(cancellationToken);

        var result = items.Select(x => new ManufacturerDto
        {
            ManufacturerId = x.ManufacturerId,
            FullName = x.FullName
        }).ToList();

        return result;
    }

}