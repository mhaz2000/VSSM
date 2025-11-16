using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.TransportMode;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class TransportModeService : ITransportModeService
{
    private readonly VSSMDbContext _dbContext;
    public TransportModeService(VSSMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(CreateTransportModeDto dto, CancellationToken cancellationToken)
    {
        var TransportMode = new TblTransportMode()
        {
            Code = dto.Code,
            FullName = dto.FullName,
            EntryDate = DateTime.Now,
            IsActive = dto.IsActive,
        };

        await _dbContext.TblTransportModes.AddAsync(TransportMode, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TransportModeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var TransportModes = _dbContext.TblTransportModes
            .AsNoTracking()
            .OrderBy(receivingMode => receivingMode.TransportModeId)
            .AsEnumerable();

        var receivingModeDto = TransportModes.Select(receivingMode => new TransportModeDto()
        {
            TransportModeId = receivingMode.TransportModeId,
            Code = receivingMode.Code,
            FullName = receivingMode.FullName,
            EntryDate = receivingMode.EntryDate,
            IsActive = receivingMode.IsActive.GetValueOrDefault()
        }).ToList();

        return receivingModeDto;
    }

    public async Task<TransportModeDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var TransportMode = await _dbContext.TblTransportModes
        .AsNoTracking()
        .SingleOrDefaultAsync(r => r.TransportModeId == id, cancellationToken);

        var TransportModeDto = TransportMode == null ? null : new TransportModeDto
        {
            TransportModeId = TransportMode.TransportModeId,
            Code = TransportMode.Code,
            FullName = TransportMode.FullName,
            EntryDate = TransportMode.EntryDate,
            IsActive = TransportMode.IsActive.GetValueOrDefault()
        };

        return TransportModeDto;
    }

    public async Task<PagingSearchOrderDto<TransportModeDto>> GetPagedListAsync(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        var query = _dbContext.TblTransportModes
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
            _ => sortOrder == "asc" ? query.OrderBy(x => x.TransportModeId) : query.OrderByDescending(x => x.TransportModeId)
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var transportModeDto = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(receivingMode => new TransportModeDto
            {
                TransportModeId = receivingMode.TransportModeId,
                Code = receivingMode.Code,
                FullName = receivingMode.FullName,
                EntryDate = receivingMode.EntryDate,
                IsActive = receivingMode.IsActive.GetValueOrDefault()
            }).ToListAsync(cancellationToken);

        var pagingSearchOrderDto = new PagingSearchOrderDto<TransportModeDto>
        {
            Items = transportModeDto,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };

        return pagingSearchOrderDto;
    }

    public async Task UpdateAsync(UpdateTransportModeDto dto, CancellationToken cancellationToken)
    {
        var transportMode = await _dbContext.TblTransportModes
            .SingleOrDefaultAsync(receivingMode => receivingMode.TransportModeId == dto.TransportModeId, cancellationToken);

        if (transportMode != null)
        {
            transportMode.Code = dto.Code;
            transportMode.FullName = dto.FullName;
            transportMode.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
