using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class ItemService : IItemService
    {
        private readonly VSSMDbContext _dbContext;
        private readonly IValidator<ItemCreateDto> _validator;
        public ItemService(VSSMDbContext dbContext, IValidator<ItemCreateDto> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<int> Create(ItemCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            /* dto to model*/
            var model = new TblItem
            {
                ItemId = dto.ItemId,
                ItemClassId = dto.ItemClassId,
                Code = dto.Code,
                EntryDate = DateTime.Now,
                FullName = dto.FullName,
                OrderId = dto.OrderId,
                IsActive = dto.IsActive,
                RequiresDiluent = dto.RequiresDiluent,
                DiluentId = dto.DiluentId,
                UniCode = dto.UniCode,
                Volume = dto.Volume,
                UserBelowZeroStore = dto.UserBelowZeroStore,
                UserAboveZeroStore = dto.UserAboveZeroStore,
                UseOrdinaryStore = dto.UseOrdinaryStore,
                UnitPrice = dto.UnitPrice,
                DayToWarmExpire = dto.DayToWarmExpire,

            };

            await _dbContext.TblItems.AddAsync(model);
            await _dbContext.SaveChangesAsync();
            return model.ItemId;
        }

        public async Task<List<ItemListDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _dbContext.TblItems
                                    .AsNoTracking()
                                    .Where(c => c.IsActive == true)
                                    .ToListAsync(cancellationToken);

            var result = items.Select(x => new ItemListDto
            {
                Id = x.ItemId,
                FullName = x.FullName,
                ItemClassId = x.ItemClassId
            }).ToList();

            return result;
        }

        public Task<List<ItemListDto>> GetAll(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemSelectDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.TblItems
                                .AsNoTracking()
                                .Where(x => x.ItemId == id)
                                .FirstOrDefaultAsync(cancellationToken);

            var dto = new ItemSelectDto
            {
                ItemId = id,
                ItemClassId = entity.ItemClassId,
                Code = entity.Code,
                FullName = entity.FullName,
                IsActive = entity.IsActive,
                OrderId = entity.OrderId,
                DayToWarmExpire = entity.DayToWarmExpire,
                DiluentId = entity.DiluentId,
                RequiresDiluent = entity.RequiresDiluent,
                UniCode = entity.UniCode,
                UnitPrice = entity.UnitPrice,
                UseOrdinaryStore = entity.UseOrdinaryStore,
                UserAboveZeroStore = entity.UserAboveZeroStore,
                UserBelowZeroStore = entity.UserBelowZeroStore,
                Volume = entity.Volume,

            };

            return dto;
        }

        public async Task<PagingSearchOrderDto<ItemListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblItem> query = _dbContext.TblItems;

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
                _ => sortOrder == "asc" ? query.OrderBy(x => x.OrderId) : query.OrderByDescending(x => x.OrderId)
            };

            // 3) Total count before paging
            paging.TotalItems = await query.CountAsync();

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            List<ItemListDto> result = new List<ItemListDto>();

            foreach (var item in items)
            {
                var diluentItem =
                    await _dbContext.TblItems.FirstOrDefaultAsync(c => c.ItemId == item.DiluentId, cancellationToken);

                var resultItem = new ItemListDto()
                {
                    Id = item.ItemId,
                    ItemClassId = item.ItemClassId,
                    FullName = item.FullName,
                    Code = item.Code,
                    UniCode = item.UniCode,
                    OrderInList = item.OrderId.GetValueOrDefault(),
                    IsActive = item.IsActive.GetValueOrDefault(),
                    DiluentId = item.DiluentId,
                    DiluentName = diluentItem?.FullName ?? "",
                };
                
                result.Add(resultItem);
            }

            return new PagingSearchOrderDto<ItemListDto>
            {
                Items = result,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<bool> Update(ItemCreateDto dto, CancellationToken cancellationToken)
        {
            /* application layer validate*/
            var result = await _validator.ValidateAsync(dto);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);


            var entity = await _dbContext.TblItems
                                .Where(x => x.ItemId == dto.ItemId)
                                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                entity.ItemClassId = dto.ItemClassId;
                entity.Code = dto.Code;
                entity.EntryDate = DateTime.Now;
                entity.FullName = dto.FullName;
                entity.OrderId = dto.OrderId;
                entity.IsActive = dto.IsActive;
                entity.RequiresDiluent = dto.RequiresDiluent;
                entity.DiluentId = dto.DiluentId;
                entity.UniCode = dto.UniCode;
                entity.Volume = dto.Volume;
                entity.UserBelowZeroStore = dto.UserBelowZeroStore;
                entity.UserAboveZeroStore = dto.UserAboveZeroStore;
                entity.UseOrdinaryStore = dto.UseOrdinaryStore;
                entity.UnitPrice = dto.UnitPrice;
                entity.DayToWarmExpire = dto.DayToWarmExpire;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<ItemListDto>> GetItemsByClassId(int classItemId)
        {
            var items = await _dbContext.TblItems
                .AsNoTracking()
                .Where(item => item.ItemClassId == classItemId)
                .Select(item => new ItemListDto()
                {
                    Id = item.ItemId,
                    ItemClassId = item.ItemClassId,
                    FullName = item.FullName,
                    Code = item.Code,
                    UniCode = item.UniCode,
                    OrderInList = item.OrderId.GetValueOrDefault(),
                    IsActive = item.IsActive.GetValueOrDefault(),
                    DiluentId = item.DiluentId,
                    DiluentName = string.Empty
                }).ToListAsync();

            return items;

        }
    }
}
