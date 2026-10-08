using FluentValidation;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.Role;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class RoleService : IRoleService
    {
        private readonly VSSMDbContext _dbContext;
        //private readonly IValidator<RoleCreateDto> _validator;
        public RoleService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Create(
            RoleCreateDto dto,
            CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Roles
                .AnyAsync(
                    x => x.Name == dto.Name,
                    cancellationToken);

            if (exists)
                return 0;

            var model = new Role
            {
                Name = dto.Name
            };

            await _dbContext.Roles.AddAsync(model, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return model.Id;
        }

        public async Task<List<RoleListDto>> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var items = await _dbContext.Roles
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

                var result = items.Select(x => new RoleListDto
                {
                    Id = x.Id,
                    Name = x.Name,

                }).ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<RoleListDto> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Roles
                                .AsNoTracking()
                                .Where(x => x.Id == id)
                                .FirstOrDefaultAsync(cancellationToken);

            var dto = new RoleListDto
            {
                Id = entity.Id,
                Name = entity.Name,
            };

            return dto;

        }

        public async Task<PagingSearchOrderDto<RoleListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<Role> query = _dbContext.Roles;

            // 1) Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.Contains(search));
            }

            // 3) Total count before paging
            paging.TotalItems = await query.CountAsync();

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new RoleListDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<RoleListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }

        public async Task<bool> Update(RoleListDto dto, CancellationToken cancellationToken)
        {

            var entity = await _dbContext.Roles
                                .Where(x => x.Id == dto.Id)
                                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                entity.Name = dto.Name;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddRoleMenu(List<RoleMenuDto> dto, CancellationToken cancellationToken)
        {
            var roleMenu = dto.Select(r => new RoleMenu
            {
                RoleId = r.RoleId,
                MenuId = r.MenuId
            }).ToList();

            await _dbContext.RoleMenus.AddRangeAsync(roleMenu);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoleMenuDto>> GetRoleMenus(int id, CancellationToken cancellationToken)
        {
            var rolemenus = await _dbContext.RoleMenus
                                .AsNoTracking()
                                .Where(x => x.RoleId == id)
                                .Select(m => new RoleMenuDto
                                {
                                    RoleId = id,
                                    MenuId = m.MenuId
                                })
                                .ToListAsync(cancellationToken);

            return rolemenus;
        }

        public async Task<bool> DeleteRoleMenus(int id, CancellationToken cancellationToken)
        {
            var rolemenus = _dbContext.RoleMenus
                                .Where(x => x.RoleId == id);

            _dbContext.RoleMenus.RemoveRange(rolemenus);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateRoleMenus(
            RoleListDto roleDto,
            List<RoleMenuDto> roleMenuDto,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Roles
                .FirstOrDefaultAsync(
                    x => x.Id == roleDto.Id,
                    cancellationToken);

            if (entity == null)
                return false;

            var duplicateName = await _dbContext.Roles
                .AnyAsync(
                    x => x.Id != roleDto.Id &&
                         x.Name == roleDto.Name,
                    cancellationToken);

            if (duplicateName)
                return false;

            entity.Name = roleDto.Name;

            var roleMenus = _dbContext.RoleMenus
                .Where(x => x.RoleId == roleDto.Id);

            _dbContext.RoleMenus.RemoveRange(roleMenus);

            var roleMenu = roleMenuDto
                .Select(r => new RoleMenu
                {
                    RoleId = r.RoleId,
                    MenuId = r.MenuId
                })
                .ToList();

            await _dbContext.RoleMenus.AddRangeAsync(
                roleMenu,
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

    }
}
