using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class ItemClassService : IItemClassService
    {
        private readonly VSSMDbContext _dbContext;
        private readonly IValidator<ItemClassCreateDto> _validator;
        public ItemClassService(VSSMDbContext dbContext, IValidator<ItemClassCreateDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<int> Create(ItemClassCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            /* dto to model*/
            var model = new TblItemsClass
            {
                Code = dto.Code,
                EntryDate = DateTime.Now,
                FullName = dto.FullName,
                Orders = dto.OrderInList,
                IsActive = dto.IsActive,
                ReqManufacturer = dto.ReqManufacturer,
                ReqBatchNo = dto.ReqBatchNo,
                ReqDosePerVial = dto.ReqDosePerVial,
                ReqExpiryDate = dto.ReqExpireDate,
                VolumeType = dto.UnitName,
                ReqVvmtype = dto.ReqVVMType
            };

            await _dbContext.TblItemsClasses.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            return model.ItemClassId;
        }

        public async Task<List<ItemClassListDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _dbContext.TblItemsClasses
                                    .AsNoTracking()
                                    .Where(c => c.IsActive == true)
                                    .ToListAsync(cancellationToken);

            var result = items.Select(x => new ItemClassListDto
            {
                Id = x.ItemClassId,
                Name = x.FullName
            }).ToList();

            return result;
        }

        public async Task<List<ItemClassListDto>> GetAll(CancellationToken cancellationToken)
        {
            try
            {


                var items = await _dbContext.TblItemsClasses
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

                var result = items.Select(x => new ItemClassListDto
                {
                    Id = x.ItemClassId,
                    Name = x.FullName,
                    Code = x.Code,
                    UnitName = x.VolumeType,
                    OrderInList = x.Orders.GetValueOrDefault(),
                    IsActive = x.IsActive.GetValueOrDefault(),
                }).ToList();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ItemClassSelectDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblItemsClasses
                                .AsNoTracking()
                                .Where(x => x.ItemClassId == id)
                                .FirstOrDefaultAsync(cancellationToken);

            var dto = new ItemClassSelectDto
            {
                ItemdClassId = entity.ItemClassId,
                Code = entity.Code,
                FullName = entity.FullName,
                IsActive = entity.IsActive.Value,
                OrderInList = entity.Orders.Value,
                UnitName = entity.VolumeType,
                ReqManufacturer = entity.ReqManufacturer,
                ReqBatchNo = entity.ReqBatchNo.Value,
                ReqDosePerVial = entity.ReqDosePerVial,
                ReqExpiryDate = entity.ReqExpiryDate,
                //ReqVvmtype = entity.ReqVVMType
            };

            return dto;

        }

        public async Task<PagingSearchOrderDto<ItemClassListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblItemsClass> query = _dbContext.TblItemsClasses;

            // 1) Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.FullName.Contains(search) || x.Code.Contains(search)
                );
            }

            // 2) Sorting
            query = sortField switch
            {
                "Name" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
                "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.Orders) : query.OrderByDescending(x => x.Orders)
            };

            // 3) Total count before paging
            paging.TotalItems = await query.CountAsync();

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new ItemClassListDto
                {
                    Id = x.ItemClassId,
                    Name = x.FullName,
                    Code = x.Code,
                    UnitName = x.VolumeType,
                    OrderInList = x.Orders.GetValueOrDefault(),
                    IsActive = x.IsActive.GetValueOrDefault(),
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<ItemClassListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<bool> Update(ItemClassCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);


            var entity = await _dbContext.TblItemsClasses
                                .Where(x => x.ItemClassId == dto.ItemClassId)
                                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                entity.Code = dto.Code;
                entity.FullName = dto.FullName;
                entity.IsActive = dto.IsActive;
                entity.Orders = dto.OrderInList;
                entity.VolumeType = dto.UnitName;
                entity.ReqManufacturer = dto.ReqManufacturer;
                entity.ReqDosePerVial = dto.ReqDosePerVial;
                entity.ReqExpiryDate = dto.ReqExpireDate;
                entity.ReqVvmtype = dto.ReqVVMType;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
