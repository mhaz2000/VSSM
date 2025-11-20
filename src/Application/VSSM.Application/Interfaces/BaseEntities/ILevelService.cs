using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Levels;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface ILevelService
    {
        // Create a new level
        Task<int> Create(LevelDto dto, CancellationToken cancellationToken);

        // Get all levels
        Task<PagingSearchOrderDto<LevelDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken cancellationToken);
        // Get a level by ID
        Task<LevelDto> GetById(int id, CancellationToken cancellationToken);

        // Update an existing level
        Task<bool> Update(LevelDto dto, CancellationToken cancellationToken);

        // Delete a level by ID
        Task<bool> Delete(int id, CancellationToken cancellationToken);
    }
}
