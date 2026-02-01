using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.Item;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IDosePerVialService
    {
        Task<int> Create(DosePerVialCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(DosePerVialCreateDto dto, CancellationToken cancellationToken);
        Task<List<DosePerVialListDto>> GetAll(CancellationToken cancellationToken);
        Task<DosePerVialSelectDto> GetById(int id, CancellationToken cancellationToken);
        Task<List<DosePerVialListDto>> GetActives(CancellationToken cancellationToken);
        Task<List<DosePerVialListDto>> GetActivesByItemId(CancellationToken cancellationToken , int itemId);
        Task<PagingSearchOrderDto<DosePerVialListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
    }
}
