using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Source;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

public class SourceService : ISourceService
{
    private readonly VSSMDbContext _db;

    public SourceService(VSSMDbContext db)
    {
        _db = db;
    }

    public async Task<SourceEditDto> GetById(int id, CancellationToken token)
    {
        var entity = await _db.TblSources
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SourceId == id, token);

        if (entity == null)
            throw new Exception("Record not found");

        return new SourceEditDto
        {
            SourceId = entity.SourceId,
            Code = entity.Code,
            FullName = entity.FullName,
            IsActive = entity.IsActive ?? false
        };
    }

    public async Task<int> Create(SourceCreateDto dto, CancellationToken token)
    {
        var model = new TblSource
        {
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive,
            EntryDate = DateTime.Now
        };

        _db.TblSources.Add(model);
        await _db.SaveChangesAsync(token);

        return model.SourceId;
    }

    public async Task<bool> Update(SourceEditDto dto, CancellationToken token)
    {
        var entity = await _db.TblSources
            .FirstOrDefaultAsync(x => x.SourceId == dto.SourceId, token);

        if (entity == null) throw new Exception("Record not found");

        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(token);
        return true;
    }

    public async Task<PagingSearchOrderDto<SourceGetDto>> GetPagedList(
        string? search, int page, int pageSize,
        string sortField, string sortOrder,
        CancellationToken token)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblSource> query = _db.TblSources;

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Code.Contains(search) || x.FullName.Contains(search));

        query = sortField switch
        {
            "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            _ => query.OrderBy(x => x.SourceId)
        };

        paging.TotalItems = await query.CountAsync(token);

        var rows = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new SourceGetDto
            {
                SourceId = x.SourceId,
                Code = x.Code,
                FullName = x.FullName,
                IsActive = x.IsActive ?? false
            })
            .ToListAsync(token);

        return new PagingSearchOrderDto<SourceGetDto>
        {
            Items = rows,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }
}
