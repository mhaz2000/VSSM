using Microsoft.EntityFrameworkCore;
using System.Linq;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.PowerSource;
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
        var transportMode = new TblTransportMode()
        {
            Code = dto.Code,
            FullName = dto.FullName,
            EntryDate = DateTime.Now,
            IsActive = dto.IsActive,
        };

        await _dbContext.TblTransportModes.AddAsync(transportMode, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TransportModeDto>> GetAll(CancellationToken cancellationToken)
    {

        var query = _dbContext.TblTransportModes;
        return await query.Select(x =>
               new TransportModeDto
               {
                   TransportModeId = x.TransportModeId,
                   Code = x.Code,
                   FullName = x.FullName,
                   EntryDate = x.EntryDate,
                   IsActive = x.IsActive.GetValueOrDefault()
               }
            ).ToListAsync(cancellationToken);
    }

    public async Task<List<TransportModeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var transportModes = _dbContext.TblTransportModes
            .AsNoTracking()
            .OrderBy(receivingMode => receivingMode.TransportModeId)
            .AsEnumerable();

        var receivingModeDto = transportModes.Select(receivingMode => new TransportModeDto()
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
        var transportMode = await _dbContext.TblTransportModes
        .AsNoTracking()
        .SingleOrDefaultAsync(r => r.TransportModeId == id, cancellationToken);

        var transportModeDto = transportMode == null ? null : new TransportModeDto
        {
            TransportModeId = transportMode.TransportModeId,
            Code = transportMode.Code,
            FullName = transportMode.FullName,
            EntryDate = transportMode.EntryDate,
            IsActive = transportMode.IsActive.GetValueOrDefault()
        };

        return transportModeDto;
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

    public async Task<List<TransportModeDto>> GetActiveTransportModes(CancellationToken cancellationToken)
    {
        var transportModes = await GetAllAsync(cancellationToken);

        transportModes = transportModes
            .Where(TransportMode => TransportMode.IsActive == true)
            .ToList();

        return transportModes;
    }
}
