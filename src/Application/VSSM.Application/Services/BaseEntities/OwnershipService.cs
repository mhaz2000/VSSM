using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Ownership;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using ValidationException = FluentValidation.ValidationException;

public class OwnershipService : IOwnershipService
{
    private readonly VSSMDbContext _db;
    private readonly IValidator<OwnershipCreateDto> _createValidator;
    private readonly IValidator<OwnershipEditDto> _editValidator;

    public OwnershipService(
        VSSMDbContext db,
        IValidator<OwnershipCreateDto> createValidator,
        IValidator<OwnershipEditDto> editValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _editValidator = editValidator;
    }

    public async Task<OwnershipEditDto> GetById(int id, CancellationToken token)
    {
        var entity = await _db.TblOwnerships
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnershipId == id, token);

        if (entity == null)
            throw new Exception("Record not found.");

        return new OwnershipEditDto
        {
            OwnershipId = entity.OwnershipId,
            Code = entity.Code,
            FullName = entity.FullName,
            IsActive = entity.IsActive ?? false
        };
    }

    public async Task<int> Create(OwnershipCreateDto dto, CancellationToken token)
    {
        // Validate
        var validation = await _createValidator.ValidateAsync(dto, token);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        string sql = @"
            INSERT INTO TblOwnership (Code, FullName, IsActive)
            VALUES (@Code, @FullName, @IsActive)
        ";

        var p1 = new SqlParameter("@Code", dto.Code ?? (object)DBNull.Value);
        var p2 = new SqlParameter("@FullName", dto.FullName ?? (object)DBNull.Value);
        var p3 = new SqlParameter("@IsActive", dto.IsActive);

        await _db.Database.ExecuteSqlRawAsync(sql, p1, p2, p3);

        return 1;
    }

    public async Task<bool> Update(OwnershipEditDto dto, CancellationToken token)
    {
        // Validate
        var validation = await _editValidator.ValidateAsync(dto, token);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        string sql = @"
            UPDATE TblOwnership
            SET Code=@Code, FullName=@FullName, IsActive=@IsActive
            WHERE OwnershipId=@Id
        ";

        var p1 = new SqlParameter("@Code", dto.Code);
        var p2 = new SqlParameter("@FullName", dto.FullName);
        var p3 = new SqlParameter("@IsActive", dto.IsActive);
        var p4 = new SqlParameter("@Id", dto.OwnershipId);

        await _db.Database.ExecuteSqlRawAsync(sql, p1, p2, p3, p4);

        return true;
    }

    public async Task<PagingSearchOrderDto<OwnershipGetDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken token)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblOwnership> query = _db.TblOwnerships;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.FullName.Contains(search) ||
                x.Code.Contains(search));
        }

        query = sortField switch
        {
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "Name" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            _ => query.OrderBy(x => x.OwnershipId)
        };

        paging.TotalItems = await query.CountAsync(token);

        var rows = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new OwnershipGetDto
            {
                OwnershipId = x.OwnershipId,
                Code = x.Code,
                FullName = x.FullName,
                IsActive = x.IsActive ?? false
            })
            .ToListAsync(token);

        return new PagingSearchOrderDto<OwnershipGetDto>
        {
            Items = rows,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }
}
