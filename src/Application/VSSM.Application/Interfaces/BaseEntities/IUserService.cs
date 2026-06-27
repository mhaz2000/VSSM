using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.User;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IUserService
    {
        Task<PagingSearchOrderDto<UserGetDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken token);

         Task<UserEditDto> GetById(int id, CancellationToken token);
        
         Task<int> Create(UserCreateDto dto, CancellationToken token);
        
         Task<bool> Update(UserEditDto dto, CancellationToken token);
        
         Task<bool> Delete(int id, CancellationToken token);

         Task<UserEditDto> GetByUsername(string username, CancellationToken token);

         Task<bool> ChangePassword(int userId, string newPassword, CancellationToken token);
    }
}
