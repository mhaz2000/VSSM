using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
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
        return entity.ManufacturerId;
    }

    // --- Get All (Paged List) ---
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
            // Search on Code, FullName, and ManufacturerId
            query = query.Where(x =>
                (x.Code != null && x.Code.Contains(search)) ||
                (x.FullName != null && x.FullName.Contains(search)) ||
                x.ManufacturerId.ToString().Contains(search)
            );
        }

        // Apply sorting
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

    // --- Get By Id ---
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

    // --- Update ---
    public async Task<bool> Update(ManufacturerDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = await _dbContext.TblManufacturers
            .FirstOrDefaultAsync(x => x.ManufacturerId == dto.ManufacturerId, cancellationToken);

        if (entity == null)
            return false;

        // Update properties
        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.EntryDate = dto.EntryDate;
        entity.ShowForVac = dto.ShowForVac;
        entity.ShowForOther = dto.ShowForOther;
        entity.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // --- Delete ---
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
}