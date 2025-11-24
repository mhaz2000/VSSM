using VSSM.Application.DTOs.BaseEntities.UserHome;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IUserHomeService
{
    Task<UserHomeDto> GetAsync();
    Task CreateOrUpdateAsync(UserHomeDto dto);
}