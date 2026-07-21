using VSSM.Application.DTOs.UidHistory;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IUidHistoryService
{
    Task<(bool Success, string Message, UidHistoryResultDto Data)>
        GetHistoryByUidAsync(string uid, int recipientId, CancellationToken token);
}
