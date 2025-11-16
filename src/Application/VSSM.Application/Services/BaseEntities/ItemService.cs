using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
        public ItemService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> Create(ItemCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<ItemListDto>> GetAll(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ItemSelectDto> GetById(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
                .Select(x => new ItemListDto
                {
                    Id = x.ItemId,
                    ItemClassId = x.ItemClassId,
                    FullName = x.FullName,
                    Code = x.Code,
                    UniCode = x.UniCode,
                    OrderInList = x.OrderId.GetValueOrDefault(),
                    IsActive = x.IsActive.GetValueOrDefault(),
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<ItemListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public Task<bool> Update(ItemCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
