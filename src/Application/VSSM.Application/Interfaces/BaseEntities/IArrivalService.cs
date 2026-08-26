using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IArrivalService
{
    Task Delete(int id);
    Task AddToTotal(int? id, float total);
    Task SubtractionToTotal(int lineId, int arrivalId);
    Task AddLine(int arrivalId, ArrivalLineDto dto, CancellationToken token);
    Task Cancel(int id, CancellationToken token);
    Task CancelLine(int arrivalLineId, CancellationToken token);
    Task Confirm(int arrivalId, ArrivalCreateDto dto, CancellationToken token);
    Task<int> Create(ArrivalCreateDto dto, CancellationToken token);
    Task<ArrivalGetDto?> GetById(int id, CancellationToken token);
    Task<PagingSearchOrderDto<ArrivalListDto>> GetPagedList(string? search, int statusFilter, int page, int pageSize, CancellationToken token);
    Task SetDraft(int arrivalId, ArrivalCreateDto dto, CancellationToken token);
    public Task<List<ArrivalCopyDto>> GetForCopy(string? reportNumber, CancellationToken token);
    Task<List<ArrivalLineDto>> GetArrivalLines(int userRecipientId);
    Task<ArrivalLineDto> GetArrivalLineById(int id);
    Task<ArrivalLineDto> GetArrivalLineByStoreIdAndItemDetailId(long storeId, long itemDetailId);
    public Task<bool> DoesHaveArrivalLineByArrivalId(int id);
    Task<string> Generate();

    Task<TblArrival?> GetEntityByIdAsync(int id, CancellationToken cancellationToken = default);

}