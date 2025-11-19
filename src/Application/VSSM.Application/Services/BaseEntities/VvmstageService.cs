using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Vvmstage;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

 public class VvmstageService : IVvmstageService
    {
        private readonly VSSMDbContext _dbContext; 
        private readonly IValidator<VvmstageDto> _validator;

        public VvmstageService(VSSMDbContext dbContext, IValidator<VvmstageDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<int> Create(VvmstageDto dto, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);


            var entity = new TblVvmstage
            {
                Code = dto.Code,
                FullName = dto.FullName,
                EntryDate = dto.EntryDate ?? DateTime.Now,
                IsActive = dto.IsActive
            };

            await _dbContext.TblVvmstages.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity.VvmstageId;
        }

        public async Task<PagingSearchOrderDto<VvmstageDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblVvmstage> query = _dbContext.TblVvmstages.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.Code != null && x.Code.Contains(search)) ||
                    (x.FullName != null && x.FullName.Contains(search)) ||
                    x.VvmstageId.ToString().Contains(search)
                );
            }

            query = sortField switch
            {
                "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
                "EntryDate" => sortOrder == "asc" ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),
                "IsActive" => sortOrder == "asc" ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.VvmstageId) : query.OrderByDescending(x => x.VvmstageId),
            };

            paging.TotalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new VvmstageDto
                {
                    VvmstageId = x.VvmstageId,
                    Code = x.Code,
                    FullName = x.FullName,
                    EntryDate = x.EntryDate,
                    IsActive = x.IsActive ?? false
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<VvmstageDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<VvmstageDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblVvmstages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.VvmstageId == id, cancellationToken);

            if (entity == null)
                return null;

            return new VvmstageDto
            {
                VvmstageId = entity.VvmstageId,
                Code = entity.Code,
                FullName = entity.FullName,
                EntryDate = entity.EntryDate,
                IsActive = entity.IsActive ?? false
            };
        }

        public async Task<bool> Update(VvmstageDto dto, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var entity = await _dbContext.TblVvmstages
                .FirstOrDefaultAsync(x => x.VvmstageId == dto.VvmstageId, cancellationToken);

            if (entity == null)
                return false;

            entity.Code = dto.Code;
            entity.FullName = dto.FullName;
            entity.EntryDate = dto.EntryDate;
            entity.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> Delete(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblVvmstages
                .FirstOrDefaultAsync(x => x.VvmstageId == id, cancellationToken);

            if (entity == null)
                return false;

            _dbContext.TblVvmstages.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }