using Microsoft.EntityFrameworkCore;
using System.Globalization;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.FiscalPeriod;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class FiscalPeriodService(VSSMDbContext dbContext) : IFiscalPeriodService
{
    public async Task<int> Create(FiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        var entity = new TblFiscalPeriod
        {
            StartDate = DateTime.Now,
            FullName = dto.FullName,
            SuggestedEndDate = dto.SuggestedEndDate.ToMiladi(),
            EntryDate = DateTime.Now,
            IsActive = dto.IsActive
        };

        var lastOne = await dbContext.TblFiscalPeriods.OrderByDescending(c => c.EntryDate).FirstOrDefaultAsync();

        if (lastOne != null)
        {
            lastOne.IsActive = false;
            lastOne.EndTime = DateTime.Now;
        }

        dbContext.TblFiscalPeriods.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.FiscalPeriodId;
    }

    public async Task<bool> Update(FiscalPeriodDto dto, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblFiscalPeriods.FindAsync(new object[] { dto.FiscalPeriodId }, cancellationToken);
        if (entity == null)
            return false;

        var lastOne = await dbContext.TblFiscalPeriods.OrderBy(c => c.EntryDate).FirstOrDefaultAsync();

        if (lastOne != null)
        {
            lastOne.IsActive = false;
            lastOne.EndTime = DateTime.Now;
        }
        entity.StartDate = DateTime.Now;
        entity.FullName = dto.FullName;
        entity.SuggestedEndDate = dto.SuggestedEndDate.ToMiladi();
        entity.EntryDate = DateTime.Now;
        entity.IsActive = dto.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FiscalPeriodDto?> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblFiscalPeriods.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null) return null;

        return new FiscalPeriodDto
        {
            FiscalPeriodId = entity.FiscalPeriodId,
            StartDate = entity.StartDate,
            EndTime = entity.EndTime,
            FullName = entity.FullName,
            SuggestedEndDate = entity.SuggestedEndDate.HasValue ? entity.SuggestedEndDate.Value.ToShamsi() : string.Empty,
            EntryDate = entity.EntryDate,
            IsActive = entity.IsActive ?? false
        };
    }

    public async Task<PagingSearchOrderDto<FiscalPeriodDto>> GetPagedList(
        string? search,
        string? status,
        int page,
        int pageSize,
        string sort,
        string direction,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblFiscalPeriod> query = dbContext.TblFiscalPeriods;

        if (!string.IsNullOrEmpty(status) && status != "all")
        {
            bool isActive = string.Equals(status, "true", StringComparison.OrdinalIgnoreCase);
            query = query.Where(x => x.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchTerm = search.Trim();
            query = query.Where(x => x.FullName != null && x.FullName.Contains(searchTerm));
        }

        bool isAscending = string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase);
        string sortLower = sort?.ToLowerInvariant() ?? string.Empty;

        query = sortLower switch
        {
            "fullname" => isAscending ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            "startdate" => isAscending ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate),
            "endtime" => isAscending ? query.OrderBy(x => x.EndTime) : query.OrderByDescending(x => x.EndTime),
            "suggestedenddate" => isAscending ? query.OrderBy(x => x.SuggestedEndDate) : query.OrderByDescending(x => x.SuggestedEndDate),
            "isactive" => isAscending ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive),
            _ => isAscending ? query.OrderBy(x => x.FiscalPeriodId) : query.OrderByDescending(x => x.FiscalPeriodId)
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        List<FiscalPeriodDto> items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new FiscalPeriodDto
            {
                FiscalPeriodId = x.FiscalPeriodId,
                FullName = x.FullName,
                StartDate = x.StartDate,
                EndTime = x.EndTime,
                SuggestedEndDate = x.SuggestedEndDate.HasValue
                    ? x.SuggestedEndDate.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)
                    : string.Empty,
                EntryDate = x.EntryDate,
                IsActive = x.IsActive ?? false
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<FiscalPeriodDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sort,
            SortOrder = direction
        };
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblFiscalPeriods
            .FirstOrDefaultAsync(
                x => x.FiscalPeriodId == id,
                cancellationToken);

        if (entity == null)
            return false;

        if (entity.IsActive == true)
            return false;

        dbContext.TblFiscalPeriods.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<FiscalPeriodDto>> GetActives(CancellationToken cancellationToken)
    {
        var items = await dbContext.TblFiscalPeriods
                                .AsNoTracking()
                                .Where(c => c.IsActive == true)
                                .ToListAsync(cancellationToken);

        var result = items.Select(x => new FiscalPeriodDto
        {
            FiscalPeriodId = x.FiscalPeriodId,
            FullName = x.FullName
        }).ToList();

        return result;
    }

    public async Task<List<FiscalPeriodDto>> GetAllAsListAsync(CancellationToken cancellationToken)
    {
        var items = await dbContext.TblFiscalPeriods
                                .AsNoTracking()
                                .ToListAsync(cancellationToken);

        var result = items.Select(x => new FiscalPeriodDto
        {
            StartDate = x.StartDate,
            EndTime = x.EndTime,
            FiscalPeriodId = x.FiscalPeriodId,
            FullName = x.FullName
        }).ToList();

        return result;
    }
}
