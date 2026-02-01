using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class DosePerVialService : IDosePerVialService
    {
        private readonly VSSMDbContext _dbContext;
        private readonly IValidator<DosePerVialCreateDto> _validator;
        public DosePerVialService(VSSMDbContext dbContext, IValidator<DosePerVialCreateDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }
        public async Task<int> Create(DosePerVialCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            /* dto to model*/
            var model = new TblDosePerVial
            {
                NumDosePerVial = dto.NumDosePerVial,
                EntryDate = DateTime.Now,
                IsActive = dto.IsActive,
            };

            await _dbContext.TblDosePerVials.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            return model.DosePerVialId;
        }

        public async Task<List<DosePerVialListDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _dbContext.TblDosePerVials
                                    .AsNoTracking()
                                    .Where(c => c.IsActive == true)
                                    .ToListAsync(cancellationToken);

            var result = items.Select(x => new DosePerVialListDto
            {

                DosePerVialId = x.DosePerVialId,
                NumDosePerVial = x.NumDosePerVial
            }).ToList();

            return result;
        }

        public async Task<List<DosePerVialListDto>> GetActivesByItemId(CancellationToken cancellationToken, int itemId)
        {

            var items = await _dbContext.TblDosePerVials
                                    .AsNoTracking().
                                    Include(z => z.TblItemDoseAssignments)
                                    .Where(c => c.IsActive == true && c.TblItemDoseAssignments.Any(z => z.ItemId == itemId))
                                    .ToListAsync(cancellationToken);

            var result = items.Select(x => new DosePerVialListDto
            {

                DosePerVialId = x.DosePerVialId,
                NumDosePerVial = x.NumDosePerVial
            }).ToList();

            return result;
        }

        public Task<List<DosePerVialListDto>> GetAll(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<DosePerVialSelectDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblDosePerVials
                                .AsNoTracking()
                                .Where(x => x.DosePerVialId == id)
                                .FirstOrDefaultAsync(cancellationToken);

            var dto = new DosePerVialSelectDto
            {
                DosePerVialId = entity.DosePerVialId,
                IsActive = entity.IsActive,
                NumDosePerVial = entity.NumDosePerVial
            };

            return dto;
        }

        public async Task<PagingSearchOrderDto<DosePerVialListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblDosePerVial> query = _dbContext.TblDosePerVials;

            // 1) Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.NumDosePerVial.ToString().Contains(search));
            }

            // 2) Sorting
            query = sortField switch
            {
                "Name" => sortOrder == "asc" ? query.OrderBy(x => x.NumDosePerVial) : query.OrderByDescending(x => x.NumDosePerVial),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.DosePerVialId) : query.OrderByDescending(x => x.DosePerVialId)
            };

            // 3) Total count before paging
            paging.TotalItems = await query.CountAsync();

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new DosePerVialListDto
                {
                    DosePerVialId = x.DosePerVialId,
                    NumDosePerVial = x.NumDosePerVial,
                    IsActive = x.IsActive.GetValueOrDefault(),
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<DosePerVialListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<bool> Update(DosePerVialCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);


            var entity = await _dbContext.TblDosePerVials
                                .Where(x => x.DosePerVialId == dto.DosePerVialId)
                                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                entity.NumDosePerVial = dto.NumDosePerVial;
                entity.IsActive = dto.IsActive;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
