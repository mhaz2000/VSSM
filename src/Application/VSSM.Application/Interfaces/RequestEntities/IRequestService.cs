using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.RequestEntities;

namespace VSSM.Application.Interfaces.RequestEntities
{
    public interface IRequestService
    {
        Task<PagingSearchOrderDto<RequestListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
    }
}
