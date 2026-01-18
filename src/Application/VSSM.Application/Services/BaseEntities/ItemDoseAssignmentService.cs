using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class ItemDoseAssignmentService : IItemDoseAssignmentService
    {
        private readonly VSSMDbContext _dbContext;
        private readonly IValidator<ItemDoseAssignmentCreateDto> _validator;
        public ItemDoseAssignmentService(VSSMDbContext dbContext, IValidator<ItemDoseAssignmentCreateDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<int> Create(ItemDoseAssignmentCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            /* dto to model*/
            var model = new TblItemDoseAssignment
            {
                ItemId = dto.ItemId,
                VialPerDoseCm3 = dto.VialPerDoseCm3,
                DoseId = dto.DoseId,
                EntryDate = DateTime.Now
            };

            await _dbContext.TblItemDoseAssignments.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            return model.ItemDoseAssignmentId;
        }

        public Task<List<ItemDoseAssignmentListDto>> GetAll(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemDoseAssignmentSelectDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblItemDoseAssignments
                                .AsNoTracking()
                                .Where(x => x.ItemDoseAssignmentId == id)
                                .FirstOrDefaultAsync(cancellationToken);

            var dto = new ItemDoseAssignmentSelectDto
            {
                ItemDoseAssignmentId = id,
                ItemId = entity.ItemId,
                DoseId = entity.DoseId,
                VialPerDoseCm3 = entity.VialPerDoseCm3
            };

            return dto;
        }

        public async Task<PagingSearchOrderDto<ItemDoseAssignmentListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblItemDoseAssignment> query = _dbContext.TblItemDoseAssignments;

            // 1) Search
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    query = query.Where(x => x.NumDosePerVial.ToString().Contains(search));
            //}

            // 2) Sorting
            //query = sortField switch
            //{
            //    "Name" => sortOrder == "asc" ? query.OrderBy(x => x.NumDosePerVial) : query.OrderByDescending(x => x.NumDosePerVial),
            //    _ => sortOrder == "asc" ? query.OrderBy(x => x.DosePerVialId) : query.OrderByDescending(x => x.DosePerVialId)
            //};

            // 3) Total count before paging
            paging.TotalItems = await query.CountAsync();

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new ItemDoseAssignmentListDto
                {
                    ItemDoseAssignmentId = x.ItemDoseAssignmentId,
                    ItemId = x.ItemId.GetValueOrDefault(),
                    ItemName = x.Item.FullName,
                    DoseId = x.ItemDoseAssignmentId,
                    DoseNum = 0,
                    VialPerDoseCm3 = x.ItemDoseAssignmentId
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<ItemDoseAssignmentListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<bool> Update(ItemDoseAssignmentCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);


            var entity = await _dbContext.TblItemDoseAssignments
                                .Where(x => x.ItemDoseAssignmentId == dto.ItemDoseAssignmentId)
                                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                entity.ItemId = dto.ItemId;
                entity.DoseId = dto.DoseId;
                entity.VialPerDoseCm3 = dto.VialPerDoseCm3;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
