using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.Menu;
using VSSM.Application.DTOs.BaseEntities.Role;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class MenuService : IMenuService
    {
        private readonly VSSMDbContext _dbContext;
        public MenuService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MenuListDto>> GetMenus(CancellationToken cancellationToken)
        {
            try
            {
                var menus = await _dbContext.Menus
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);

                var result = menus.Select(x => new MenuListDto
                {
                    Id = x.Id,
                    Title = x.Ttitle,
                    Controller = x.Controller,
                    Action = x.Action,
                    Icon = x.Icon,
                    MenuOrder = x.MenuOrder,
                    ParentId = x.ParentId,

                }).ToList();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
