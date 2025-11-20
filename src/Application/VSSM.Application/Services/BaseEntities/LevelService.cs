using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Levels;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class LevelService : ILevelService
    {
        private readonly VSSMDbContext _dbContext;
        private readonly IValidator<LevelDto> _validator;

        public LevelService(VSSMDbContext dbContext, IValidator<LevelDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<int> Create(LevelDto dto, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var maxId = await _dbContext.TblLevels.MaxAsync(x => (int?)x.LevelId) ?? 0;

            var entity = new TblLevel
            {
                LevelId = maxId + 1,
                ShipCount = dto.ShipCount,
                SafetyStockPercentage = dto.SafetyStockPercentage,
                SafetyStockMonth = dto.SafetyStockMonth,
                Note = dto.Note,
                MaxDay = dto.MaxDay
            };

            await _dbContext.TblLevels.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity.LevelId;
        }

        public async Task<PagingSearchOrderDto<LevelDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblLevel> query = _dbContext.TblLevels.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Note.Contains(search) ||
                    x.LevelId.ToString().Contains(search)
                );
            }

            query = sortField switch
            {
                "ShipCount" => sortOrder == "asc"
                    ? query.OrderBy(x => x.ShipCount)
                    : query.OrderByDescending(x => x.ShipCount),
                "SafetyStockPercentage" => sortOrder == "asc"
                    ? query.OrderBy(x => x.SafetyStockPercentage)
                    : query.OrderByDescending(x => x.SafetyStockPercentage),
                "SafetyStockMonth" => sortOrder == "asc"
                    ? query.OrderBy(x => x.SafetyStockMonth)
                    : query.OrderByDescending(x => x.SafetyStockMonth),
                "MaxDay" => sortOrder == "asc" ? query.OrderBy(x => x.MaxDay) : query.OrderByDescending(x => x.MaxDay),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.LevelId) : query.OrderByDescending(x => x.LevelId),
            };

            paging.TotalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new LevelDto
                {
                    LevelId = x.LevelId,
                    ShipCount = x.ShipCount,
                    SafetyStockPercentage = x.SafetyStockPercentage,
                    SafetyStockMonth = x.SafetyStockMonth,
                    Note = x.Note,
                    MaxDay = x.MaxDay
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<LevelDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<LevelDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.LevelId == id, cancellationToken);

            if (entity == null)
                return null;

            return new LevelDto
            {
                LevelId = entity.LevelId,
                ShipCount = entity.ShipCount,
                SafetyStockPercentage = entity.SafetyStockPercentage,
                SafetyStockMonth = entity.SafetyStockMonth,
                Note = entity.Note,
                MaxDay = entity.MaxDay
            };
        }

        public async Task<bool> Update(LevelDto dto, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var entity = await _dbContext.TblLevels
                .FirstOrDefaultAsync(x => x.LevelId == dto.LevelId, cancellationToken);

            if (entity == null)
                return false;

            entity.ShipCount = dto.ShipCount;
            entity.SafetyStockPercentage = dto.SafetyStockPercentage;
            entity.SafetyStockMonth = dto.SafetyStockMonth;
            entity.Note = dto.Note;
            entity.MaxDay = dto.MaxDay;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> Delete(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblLevels
                .FirstOrDefaultAsync(x => x.LevelId == id, cancellationToken);

            if (entity == null)
                return false;

            _dbContext.TblLevels.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}