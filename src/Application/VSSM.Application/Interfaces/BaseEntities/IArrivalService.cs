using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Arrival;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IArrivalService
    {
        Task AddLine(int arrivalId, ArrivalLineDto dto, CancellationToken token);
        Task Cancel(int id, CancellationToken token);
        Task CancelLine(int arrivalLineId, CancellationToken token);
        Task Confirm(int arrivalId, CancellationToken token);
        Task<int> Create(ArrivalCreateDto dto, CancellationToken token);
        Task<ArrivalGetDto?> GetById(int id, CancellationToken token);
        Task<PagingSearchOrderDto<ArrivalListDto>> GetPagedList(string? search, int statusFilter, int page, int pageSize, CancellationToken token);
        Task SetDraft(int arrivalId, CancellationToken token);
        public Task<List<ArrivalCopyDto>> GetForCopy(string? reportNumber, CancellationToken token);
    }
}