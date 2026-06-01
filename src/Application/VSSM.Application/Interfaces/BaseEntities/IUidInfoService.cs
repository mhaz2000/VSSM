using VSSM.Application.DTOs.BaseEntities.Arrival;

namespace VSSM.Application.Interfaces.BaseEntities;


public interface IUidInfoService
{

    Task<List<UidInfoDto>> GetAvailableByBatchNum(string batchNum, CancellationToken token);


    Task SetArrivalFkForBatchNums(
        List<string> batchNums,
        int arrivalId,
        int? dosePerVialId,
        int? storeId,
        int? itemId,
        CancellationToken token);


    Task ClearArrivalFkByArrivalId(int arrivalId, CancellationToken token);

    Task ClearArrivalFkByLineId(int arrivalLineId, CancellationToken token);
}
