using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface ITblRoadTypeService
    {
        Task<int> Create(TblRoadTypeCreateDto dto, CancellationToken cancellationToken);
        Task<bool> Update(TblRoadTypeEditDto dto, CancellationToken cancellationToken);
        Task<List<TblRoadTypeGetDto>> GetAll(CancellationToken cancellationToken);
        Task<TblRoadTypeEditDto> GetById(int id, CancellationToken cancellationToken);
    }

}
