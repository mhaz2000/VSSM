using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.Menu;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IMenuService
    {
        Task<List<MenuListDto>> GetMenus(CancellationToken cancellationToken);
    }
}
