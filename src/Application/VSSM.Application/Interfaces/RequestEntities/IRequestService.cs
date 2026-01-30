using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Domain.Enums;

namespace VSSM.Application.Interfaces.RequestEntities
{
    public interface IRequestService
    {
        Task<PagingSearchOrderDto<RequestListDto>> GetPagedList(string? search, int page, int pageSize, 
            string sortField, string sortOrder, int currentRecipientId, CancellationToken cancellationToken);
        Task<int> Create(RequestCreateDto dto, CancellationToken token);
        Task AddLine(int requestId, RequestLineDto dto, CancellationToken token);
        Task<RequestGetDto?> GetById(int id, CancellationToken token);
        Task<bool> ChangeStatus(int id, RequestStatusEnum status, string note, CancellationToken token);
        Task Delete(int id);
        Task DeleteLine(int requestLineId, CancellationToken token);

        Task<PagingSearchOrderDto<RequestListDto>> GetReceivedRequestsPagedList(string? search, int page, int pageSize,
                string sortField, string sortOrder,
                int currentRecipientId, CancellationToken cancellationToken);
    }
}
