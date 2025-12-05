using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.PowerSource;
using VSSM.Application.DTOs.BaseEntities.Store;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IPowerSourceService
    {
        Task<List<PowerSourceGetDto>> GetAll(CancellationToken cancellationToken);
        public Task<PagingSearchOrderDto<PowerSourceGetDto>> GetPagedList(
       string? search, int page, int pageSize, string sortField, string sortOrder,
       CancellationToken token);
        public Task<bool> Update(PowerSourceEditDto dto, CancellationToken token);
        public Task<int> Create(PowerSourceCreateDto dto, CancellationToken token);
        public Task<PowerSourceEditDto> GetById(int id, CancellationToken token);
    }
}
