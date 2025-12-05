using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.RoadType;
using VSSM.Application.DTOs.BaseEntities.TransportMode;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IRoadTypeService
    {
        Task<List<RoadTypeGetDto>> GetAll(CancellationToken cancellationToken);
        Task<int> Create(RoadTypeCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(RoadTypeEditDto dto, CancellationToken cancellationToken);
        Task<PagingSearchOrderDto<RoadTypeGetDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken);
        Task<RoadTypeEditDto> GetById(int id, CancellationToken cancellationToken);
    }

}
