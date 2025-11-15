using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ReceivingMode;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ReceivingModeService : IReceivingModeService
{
    private readonly VSSMDbContext _dbContext;
    public ReceivingModeService(VSSMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(CreateReceivingModeDto dto, CancellationToken cancellationToken)
    {
        var receivingMode = new TblReceivingMode()
        {
            Code = dto.Code,
            FullName = dto.FullName,
            EntryDate = DateTime.Now,
            IsActive = dto.IsActive,
        };

        await _dbContext.TblReceivingModes.AddAsync(receivingMode, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ReceivingModeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var receivingModes = _dbContext.TblReceivingModes
            .AsNoTracking()
            .OrderBy(receivingMode => receivingMode.ReceivingModeId)
            .AsEnumerable();

        var receivingModeDto = receivingModes.Select(receivingMode => new ReceivingModeDto()
        {
            ReceivingModeId = receivingMode.ReceivingModeId,
            Code = receivingMode.Code,
            FullName = receivingMode.FullName,
            EntryDate = receivingMode.EntryDate,
            IsActive = receivingMode.IsActive.GetValueOrDefault()
        }).ToList();

        return receivingModeDto;
    }

    public async Task<ReceivingModeDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var receivingMode = await _dbContext.TblReceivingModes
        .AsNoTracking()
        .SingleOrDefaultAsync(r => r.ReceivingModeId == id, cancellationToken);

        var receivingModeDto = receivingMode == null ? null : new ReceivingModeDto
        {
            ReceivingModeId = receivingMode.ReceivingModeId,
            Code = receivingMode.Code,
            FullName = receivingMode.FullName,
            EntryDate = receivingMode.EntryDate,
            IsActive = receivingMode.IsActive.GetValueOrDefault()
        };

        return receivingModeDto;
    }

    public async Task<PagingSearchOrderDto<ReceivingModeDto>> GetPagedListAsync(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        var query = _dbContext.TblReceivingModes
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.FullName.Contains(search) || x.Code.Contains(search));
        }

        query = sortField switch
        {
            "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            _ => sortOrder == "asc" ? query.OrderBy(x => x.ReceivingModeId) : query.OrderByDescending(x => x.ReceivingModeId)
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var receivingModeDto = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(receivingMode => new ReceivingModeDto
            {
                ReceivingModeId = receivingMode.ReceivingModeId,
                Code = receivingMode.Code,
                FullName = receivingMode.FullName,
                EntryDate = receivingMode.EntryDate,
                IsActive = receivingMode.IsActive.GetValueOrDefault()
            }).ToListAsync(cancellationToken);

        var pagingSearchOrderDto = new PagingSearchOrderDto<ReceivingModeDto>
        {
            Items = receivingModeDto,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };

        return pagingSearchOrderDto;
    }

    public async Task UpdateAsync(UpdateReceivingModeDto dto, CancellationToken cancellationToken)
    {
        var receivingMode = await _dbContext.TblReceivingModes
            .SingleOrDefaultAsync(receivingMode => receivingMode.ReceivingModeId == dto.ReceivingModeId, cancellationToken);

        if (receivingMode != null)
        {
            receivingMode.Code = dto.Code;
            receivingMode.FullName = dto.FullName;
            //receivingMode.EntryDate = dto.EntryDate;
            receivingMode.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
