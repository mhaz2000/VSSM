using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Climate;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class ClimateService : IClimateService
{
    private readonly VSSMDbContext _dbContext;
    public ClimateService(VSSMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(CreateClimateDto dto, CancellationToken cancellationToken)
    {
        var climate = new TblClimate()
        {
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive,
        };

        await _dbContext.TblClimates.AddAsync(climate, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ClimateDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var climates = _dbContext.TblClimates
            .AsNoTracking()
            .OrderBy(climate => climate.ClimateId)
            .AsEnumerable();

        var climateDto = climates.Select(receivingMode => new ClimateDto()
        {
            ClimateId = receivingMode.ClimateId,
            Code = receivingMode.Code,
            FullName = receivingMode.FullName,
            IsActive = receivingMode.IsActive.GetValueOrDefault()
        }).ToList();

        return climateDto;
    }

    public async Task<ClimateDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var TransportMode = await _dbContext.TblClimates
        .AsNoTracking()
        .SingleOrDefaultAsync(climate => climate.ClimateId == id, cancellationToken);

        var climateDto = TransportMode == null ? null : new ClimateDto
        {
            ClimateId = TransportMode.ClimateId,
            Code = TransportMode.Code,
            FullName = TransportMode.FullName,
            IsActive = TransportMode.IsActive.GetValueOrDefault()
        };

        return climateDto;
    }

    public async Task<PagingSearchOrderDto<ClimateDto>> GetPagedListAsync(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        var query = _dbContext.TblClimates
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
            _ => sortOrder == "asc" ? query.OrderBy(x => x.ClimateId) : query.OrderByDescending(x => x.ClimateId)
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var climateDto = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(receivingMode => new ClimateDto
            {
                ClimateId = receivingMode.ClimateId,
                Code = receivingMode.Code,
                FullName = receivingMode.FullName,
                IsActive = receivingMode.IsActive.GetValueOrDefault()
            }).ToListAsync(cancellationToken);

        var pagingSearchOrderDto = new PagingSearchOrderDto<ClimateDto>
        {
            Items = climateDto,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };

        return pagingSearchOrderDto;
    }

    public async Task UpdateAsync(UpdateClimateDto dto, CancellationToken cancellationToken)
    {
        var climate = await _dbContext.TblClimates
            .SingleOrDefaultAsync(receivingMode => receivingMode.ClimateId == dto.ClimateId, cancellationToken);

        if (climate != null)
        {
            climate.Code = dto.Code;
            climate.FullName = dto.FullName;
            climate.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
