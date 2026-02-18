using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.Common.Interfaces;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services
{
    public class MenuPermissionService : IMenuPermissionService
    {
        private readonly VSSMDbContext _db;

        public MenuPermissionService(VSSMDbContext db)
        {
            _db = db;
        }
        public async Task<bool> HasAccessAsync(int roleId, string controller)
        {
            return await _db.RoleMenus
            .AnyAsync(rm =>
                rm.RoleId == roleId &&
                rm.Menu.Controller == controller);
        }
    }
}
