using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.Arrivals;
using VSSM.Domain.Enums;


namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IArrivalService
    {
        Task<IList<ArrivalListDto>> GetList(
            string search,
            ArrivalStatusFilter status,
            int page,
            int pageSize,
            CancellationToken token);

        Task<ArrivalDto> GetById(int id, CancellationToken token);

        Task<int> Save(ArrivalCreateEditDto dto, CancellationToken token);

        Task<bool> Delete(int id, CancellationToken token);          // IsCancelled = true
        Task<bool> DeleteLine(int lineId, CancellationToken token);  // IsCancelled روی Line (در این مدل یعنی حذف واقعی)
    }
}
