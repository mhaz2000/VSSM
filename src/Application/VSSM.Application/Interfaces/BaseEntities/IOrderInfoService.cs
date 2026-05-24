using System.Xml.Linq;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.DTOs.BaseEntities.OrderInfo;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IOrderInfoService
{
    Task UploadAsync(XDocument file, string fileName, int userId);

    Task<(List<OrderInfoDto> data, int count)> GetPagedAsync(string? search, int page, int pageSize,
        string sortField, string sortOrder, CancellationToken cancellationToken);
    Task RemoveAsync(int id, CancellationToken cancellationToken);
}
