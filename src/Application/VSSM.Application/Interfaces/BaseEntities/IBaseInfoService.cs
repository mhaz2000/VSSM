using VSSM.Application.DTOs.BaseEntities.BaseInfo;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IBaseInfoService
{
    Task AddAsync(BaseInfoInputDto model, int currentUserId);
    Task<BaseInfoDto> GetByIdAsync(int id);
    Task<(List<BaseInfoDto> data, int count)> GetPagedBaseInfoAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task RemoveAsync(int id);
    Task UpdateAsync(BaseInfoInputDto model, int currentUserId);

    Task<List<BaseInfoDto>> GetByParentIdAsync(int parentId, CancellationToken cancellationToken = default);
}
