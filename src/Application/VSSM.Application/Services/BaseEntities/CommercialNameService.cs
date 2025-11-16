using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.CommercialName;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class CommercialNameService : ICommercialNameService
    {
        private readonly VSSMDbContext _dbContext;
        private readonly IValidator<CommercialNameDto> _validator;

        public CommercialNameService(VSSMDbContext dbContext, IValidator<CommercialNameDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        // Create
        public async Task<int> Create(CommercialNameDto dto, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var entity = new TblCommercialName
            {
                FullName = dto.FullName,
                Code = dto.Code,
                IsActive = dto.IsActive,
                EntryDate = dto.EntryDate
            };

            await _dbContext.TblCommercialNames.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity.CommercialNameId;
        }

        // Get All with Paging
        public async Task<PagingSearchOrderDto<CommercialNameDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblCommercialName> query = _dbContext.TblCommercialNames.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    x.Code.Contains(search) ||
                    x.CommercialNameId.ToString().Contains(search)
                );
            }

            query = sortField switch
            {
                "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
                "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                "EntryDate" => sortOrder == "asc" ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.CommercialNameId) : query.OrderByDescending(x => x.CommercialNameId),
            };

            paging.TotalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new CommercialNameDto
                {
                    CommercialNameId = x.CommercialNameId,
                    FullName = x.FullName,
                    Code = x.Code,
                    IsActive = x.IsActive ?? false,
                    EntryDate = x.EntryDate
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<CommercialNameDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        // Get By Id
        public async Task<CommercialNameDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblCommercialNames
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CommercialNameId == id, cancellationToken);

            if (entity == null)
                return null;

            return new CommercialNameDto
            {
                CommercialNameId = entity.CommercialNameId,
                FullName = entity.FullName,
                Code = entity.Code,
                IsActive = entity.IsActive ?? false,
                EntryDate = entity.EntryDate
            };
        }

        // Update
        public async Task<bool> Update(CommercialNameDto dto, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var entity = await _dbContext.TblCommercialNames
                .FirstOrDefaultAsync(x => x.CommercialNameId == dto.CommercialNameId, cancellationToken);

            if (entity == null)
                return false;

            entity.FullName = dto.FullName;
            entity.Code = dto.Code;
            entity.IsActive = dto.IsActive;
            entity.EntryDate = dto.EntryDate;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Delete
        public async Task<bool> Delete(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblCommercialNames
                .FirstOrDefaultAsync(x => x.CommercialNameId == id, cancellationToken);

            if (entity == null)
                return false;

            _dbContext.TblCommercialNames.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }