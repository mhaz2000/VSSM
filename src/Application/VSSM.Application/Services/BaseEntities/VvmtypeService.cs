using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.Vvmtype;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class VvmtypeService : IVvmtypeService
{
    private readonly VSSMDbContext _dbContext;
    private readonly IValidator<VvmtypeDto> _validator;

    public VvmtypeService(VSSMDbContext dbContext, IValidator<VvmtypeDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Create(VvmtypeDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = new TblVvmtype
        {
            Code = dto.Code,
            FullName = dto.FullName,
            CanUserChange = dto.CanUserChange,
            EntryDate = dto.EntryDate ?? DateTime.Now,
            IsActive = dto.IsActive
        };

        await _dbContext.TblVvmtypes.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.VvmtypeId;
    }

    public async Task<PagingSearchOrderDto<VvmtypeDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblVvmtype> query = _dbContext.TblVvmtypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                (x.Code != null && x.Code.Contains(search)) ||
                (x.FullName != null && x.FullName.Contains(search)) ||
                x.VvmtypeId.ToString().Contains(search)
            );
        }

        query = sortField switch
        {
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            "CanUserChange" => sortOrder == "asc" ? query.OrderBy(x => x.CanUserChange) : query.OrderByDescending(x => x.CanUserChange),
            "EntryDate" => sortOrder == "asc" ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),
            "IsActive" => sortOrder == "asc" ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive),
            _ => sortOrder == "asc" ? query.OrderBy(x => x.VvmtypeId) : query.OrderByDescending(x => x.VvmtypeId),
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new VvmtypeDto
            {
                VvmtypeId = x.VvmtypeId,
                Code = x.Code,
                FullName = x.FullName,
                CanUserChange = x.CanUserChange ?? false,
                EntryDate = x.EntryDate,
                IsActive = x.IsActive ?? false
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<VvmtypeDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<VvmtypeDto> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblVvmtypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VvmtypeId == id, cancellationToken);

        if (entity == null)
            return null;

        return new VvmtypeDto
        {
            VvmtypeId = entity.VvmtypeId,
            Code = entity.Code,
            FullName = entity.FullName,
            CanUserChange = entity.CanUserChange ?? false,
            EntryDate = entity.EntryDate,
            IsActive = entity.IsActive ?? false
        };
    }

    public async Task<bool> Update(VvmtypeDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = await _dbContext.TblVvmtypes
            .FirstOrDefaultAsync(x => x.VvmtypeId == dto.VvmtypeId, cancellationToken);

        if (entity == null)
            return false;

        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.CanUserChange = dto.CanUserChange;
        entity.EntryDate = dto.EntryDate;
        entity.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblVvmtypes
            .FirstOrDefaultAsync(x => x.VvmtypeId == id, cancellationToken);

        if (entity == null)
            return false;

        _dbContext.TblVvmtypes.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<VvmtypeDto>> GetActives(CancellationToken cancellationToken)
    {
        var items = await _dbContext.TblVvmtypes
                                 .AsNoTracking()
                                 .Where(c => c.IsActive == true)
                                 .ToListAsync(cancellationToken);

        var result = items.Select(x => new VvmtypeDto
        {
            VvmtypeId = x.VvmtypeId,
            Code = x.Code,
            FullName = x.FullName
        }).ToList();

        return result;
    }
    
}