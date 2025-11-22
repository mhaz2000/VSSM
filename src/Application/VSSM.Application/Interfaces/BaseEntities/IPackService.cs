using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Pack;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IPackService
    {
        // GET BY ID
        public Task<PackEditDto> GetById(int id, CancellationToken token);

        // CREATE
        public Task<int> Create(PackCreateDto dto, CancellationToken token);

        // UPDATE
        public Task<bool> Update(PackEditDto dto, CancellationToken token);

        // PAGED LIST
        public Task<PagingSearchOrderDto<PackGetDto>> GetPagedList(
            string? search, int page, int pageSize, string sortField, string sortOrder,
            CancellationToken token);
    }
}
