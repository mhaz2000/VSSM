using FluentValidation;
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

        public Task<PagingSearchOrderDto<ItemListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(ItemCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
