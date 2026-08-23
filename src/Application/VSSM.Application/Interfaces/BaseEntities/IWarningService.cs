using VSSM.Application.DTOs.BaseEntities.Warnings;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IWarningService
    {
        Task<(List<HighItemsMaxDto> data, int count)> GetHighItemsMaxAsync(string? search, int page, int pageSize, string sortField, string sortOrder, int recipientId, CancellationToken cancellationToken);
        Task<(List<LowItemsMinDto> data, int count)> GetLowItemsMinAsync(string? search, int page, int pageSize, string sortField, string sortOrder, int recipientId, CancellationToken cancellationToken);
        Task<(List<CloseToExpirationDto> Data, int Count)> GetCloseToExpirationAsync(
                 string? search,
                 int page,
                 int pageSize,
                 string sortField,
                 string sortOrder,
                 int recipientId,
                 CancellationToken cancellationToken);
    }
}
