using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.FiscalPeriod;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace VSSM.Application.Services.BaseEntities
{
    public class FiscalPeriodService : IFiscalPeriodService
    {
        private readonly VSSMDbContext _dbContext;

        public FiscalPeriodService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Create(FiscalPeriodDto dto, CancellationToken cancellationToken)
        {
            var entity = new TblFiscalPeriod
            {
                StartDate = dto.StartDate,
                EndTime = dto.EndTime,
                FullName = dto.FullName,
                SuggestedEndDate = dto.SuggestedEndDate,
                EntryDate = dto.EntryDate ?? DateTime.Now,
                IsActive = dto.IsActive
            };

            _dbContext.TblFiscalPeriods.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return entity.FiscalPeriodId;
        }

        public async Task<bool> Update(FiscalPeriodDto dto, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblFiscalPeriods.FindAsync(new object[] { dto.FiscalPeriodId }, cancellationToken);
            if (entity == null)
                return false;

            entity.StartDate = dto.StartDate;
            entity.EndTime = dto.EndTime;
            entity.FullName = dto.FullName;
            entity.SuggestedEndDate = dto.SuggestedEndDate;
            entity.EntryDate = dto.EntryDate;
            entity.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<FiscalPeriodDto?> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblFiscalPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null) return null;

            return new FiscalPeriodDto
            {
                FiscalPeriodId = entity.FiscalPeriodId,
                StartDate = entity.StartDate,
                EndTime = entity.EndTime,
                FullName = entity.FullName,
                SuggestedEndDate = entity.SuggestedEndDate,
                EntryDate = entity.EntryDate,
                IsActive = entity.IsActive ?? false
            };
        }

        public async Task<PagingSearchOrderDto<FiscalPeriodDto>> GetPagedList(
            string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblFiscalPeriod> query = _dbContext.TblFiscalPeriods;

            // 1) Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.FullName.Contains(search));
            }

            // 2) Sorting
            query = sortField switch
            {
                "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
                "StartDate" => sortOrder == "asc" ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate),
                "EndTime" => sortOrder == "asc" ? query.OrderBy(x => x.EndTime) : query.OrderByDescending(x => x.EndTime),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.FiscalPeriodId) : query.OrderByDescending(x => x.FiscalPeriodId)
            };

            // 3) Total count
            paging.TotalItems = await query.CountAsync(cancellationToken);

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new FiscalPeriodDto
                {
                    FiscalPeriodId = x.FiscalPeriodId,
                    FullName = x.FullName,
                    StartDate = x.StartDate,
                    EndTime = x.EndTime,
                    SuggestedEndDate = x.SuggestedEndDate,
                    EntryDate = x.EntryDate,
                    IsActive = x.IsActive ?? false
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<FiscalPeriodDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<bool> Delete(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblFiscalPeriods.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null) return false;

            _dbContext.TblFiscalPeriods.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
