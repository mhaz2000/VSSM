using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.BaseEntities.RoadType;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IRoadTypeService
    {
        Task<int> Create(RoadTypeCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(RoadTypeEditDto dto, CancellationToken cancellationToken);
        Task<List<RoadTypeGetDto>> GetAll(CancellationToken cancellationToken);
        Task<RoadTypeEditDto> GetById(int id, CancellationToken cancellationToken);
    }

}
