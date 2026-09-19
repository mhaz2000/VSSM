using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.DTOs.BaseEntities.UserManagement;

namespace VSSM.Application.Interfaces.BaseEntities;

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
    Task SetMustChangePasswordAsync(int userId, bool value, CancellationToken token);

    /// <summary>True when the plaintext password matches the stored PBKDF2 hash.</summary>
    Task<bool> VerifyPassword(int userId, string password, CancellationToken token);

    /// <summary>Re-hashes a legacy plaintext password on first successful login.</summary>
    Task<bool> UpgradePasswordToHash(int userId, string plaintextPassword, CancellationToken token);

    /// <summary>Hashes all remaining plaintext passwords in the database at startup (idempotent).</summary>
    Task UpgradeAllPlaintextPasswordsAsync(CancellationToken token);

    Task<UserSearchResultDto> SearchUsersWithRecipientAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ChangeRecipientAsync(int userId, int newRecipientId, CancellationToken cancellationToken = default);
    Task<UserWithRecipientDto> GetUserWithRecipientAsync(int userId, CancellationToken cancellationToken = default);

    Task<List<BaseInfoDto>> GetAdminUsersAsBaseInfoAsync(
        CancellationToken cancellationToken = default);
}
