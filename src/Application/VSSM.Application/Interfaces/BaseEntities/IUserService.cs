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
        
         Task<bool> ToggleActiveness(int id, CancellationToken token);

         Task<UserEditDto> GetByUsername(string username, CancellationToken token);

         Task<bool> ChangePassword(int userId, string newPassword, CancellationToken token);

         /// <summary>True when the plaintext password matches the stored PBKDF2 hash.</summary>
         Task<bool> VerifyPassword(int userId, string password, CancellationToken token);

         /// <summary>Re-hashes a legacy plaintext password on first successful login.</summary>
         Task<bool> UpgradePasswordToHash(int userId, string plaintextPassword, CancellationToken token);
    }
}
