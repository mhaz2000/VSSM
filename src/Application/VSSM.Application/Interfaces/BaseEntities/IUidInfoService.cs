using VSSM.Application.DTOs.BaseEntities.Arrival;

namespace VSSM.Application.Interfaces.BaseEntities;


public interface IUidInfoService
{

    Task<List<UidInfoDto>> GetAvailableByBatchNum(
        string batchNum,
        int recipientId,  
        CancellationToken token);

    Task SetArrivalFkForBatchNums(
        List<string> batchNums,
        int arrivalId,
        int? doseCount,
        int? storeId,
        int? itemId,
        bool? isHalal,
        CancellationToken token);


    Task ClearArrivalFkByArrivalId(int arrivalId, CancellationToken token);

    Task ClearArrivalFkByLineId(int arrivalLineId, CancellationToken token);
}
