using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.RequestEntities;

namespace VSSM.Application.Interfaces.RequestEntities
{
    public interface IRequestService
    {
        Task<PagingSearchOrderDto<RequestListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
        Task<int> Create(RequestCreateDto dto, CancellationToken token);
        Task AddLine(int requestId, RequestLineDto dto, CancellationToken token);
        Task<RequestGetDto?> GetById(int id, CancellationToken token);

    }
}
