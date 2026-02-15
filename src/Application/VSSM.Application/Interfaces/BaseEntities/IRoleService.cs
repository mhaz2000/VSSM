using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Role;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IRoleService
    {
        Task<int> Create(RoleCreateDto dto, CancellationToken cancellationToken);
        Task<List<RoleListDto>> GetAll(CancellationToken cancellationToken);
        Task<RoleListDto> GetById(int id, CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<RoleListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
        Task<bool> Update(RoleListDto dto, CancellationToken cancellationToken);
        Task<bool> AddRoleMenu(List<RoleMenuDto> dto, CancellationToken cancellationToken);
        Task<List<RoleMenuDto>> GetRoleMenus(int id, CancellationToken cancellationToken);
        Task<bool> DeleteRoleMenus(int id, CancellationToken cancellationToken);
        Task<bool> UpdateRoleMenus(RoleListDto roleDto, List<RoleMenuDto> roleMenuDto, CancellationToken cancellationToken);
    }
}
