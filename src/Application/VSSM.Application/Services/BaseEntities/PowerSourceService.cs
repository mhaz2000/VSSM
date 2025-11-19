using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.PowerSource;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

public class PowerSourceService : IPowerSourceService
{
    private readonly VSSMDbContext _db;

    public PowerSourceService(VSSMDbContext db)
    {
        _db = db;
    }

    public async Task<PowerSourceEditDto> GetById(int id, CancellationToken token)
    {
        var entity = await _db.TblPowerSources
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PowerSourceId == id, token);

        if (entity == null)
            throw new Exception("Record not found.");

        return new PowerSourceEditDto
        {
            PowerSourceId = entity.PowerSourceId,
            Code = entity.Code,
            FullName = entity.FullName,
            IsActive = entity.IsActive ?? false
        };
    }

    public async Task<int> Create(PowerSourceCreateDto dto, CancellationToken token)
    {
        string sql = @"
            INSERT INTO TblPowerSource (Code, FullName, IsActive)
            VALUES (@Code, @FullName, @IsActive)
        ";

        var p1 = new SqlParameter("@Code", dto.Code ?? (object)DBNull.Value);
        var p2 = new SqlParameter("@FullName", dto.FullName ?? (object)DBNull.Value);
        var p3 = new SqlParameter("@IsActive", dto.IsActive);

        await _db.Database.ExecuteSqlRawAsync(sql, p1, p2, p3);

        return 1;
    }

    public async Task<bool> Update(PowerSourceEditDto dto, CancellationToken token)
    {
        string sql = @"
            UPDATE TblPowerSource
            SET Code=@Code, FullName=@FullName, IsActive=@IsActive
            WHERE PowerSourceId=@Id
        ";

        var p1 = new SqlParameter("@Code", dto.Code);
        var p2 = new SqlParameter("@FullName", dto.FullName);
        var p3 = new SqlParameter("@IsActive", dto.IsActive);
        var p4 = new SqlParameter("@Id", dto.PowerSourceId);

        await _db.Database.ExecuteSqlRawAsync(sql, p1, p2, p3, p4);

        return true;
    }

    public async Task<PagingSearchOrderDto<PowerSourceGetDto>> GetPagedList(
        string? search, int page, int pageSize, string sortField, string sortOrder,
        CancellationToken token)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblPowerSource> query = _db.TblPowerSources;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.FullName.Contains(search) || x.Code.Contains(search));
        }

        query = sortField switch
        {
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "Name" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            _ => query.OrderBy(x => x.PowerSourceId)
        };

        paging.TotalItems = await query.CountAsync(token);

        var rows = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new PowerSourceGetDto
            {
                PowerSourceId = x.PowerSourceId,
                Code = x.Code,
                FullName = x.FullName,
                IsActive = x.IsActive ?? false
            })
            .ToListAsync(token);

        return new PagingSearchOrderDto<PowerSourceGetDto>
        {
            Items = rows,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }
}
