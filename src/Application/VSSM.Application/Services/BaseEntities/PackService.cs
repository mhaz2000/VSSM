using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Pack;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class PackService : IPackService
    {
        private readonly VSSMDbContext _db;

        public PackService(VSSMDbContext db)
        {
            _db = db;
        }

        // GET BY ID
        public async Task<PackEditDto> GetById(int id, CancellationToken token)
        {
            var entity = await _db.TblPacks.FirstOrDefaultAsync(x => x.PackId == id, token);
            if (entity == null) return null;

            return new PackEditDto
            {
                PackId = entity.PackId,
                Code = entity.Code,
                FullName = entity.FullName,
                NetVolume = entity.NetVolume,
                ExternalSize = entity.ExternalSize,
                PackWeight = entity.PackWeight,
                IsActive = entity.IsActive ?? false,
                UseForVacDL = entity.UseForVacDl.Value,
                UseForOS = entity.UseForOs.Value
            };
        }

        // CREATE
        public async Task<int> Create(PackCreateDto dto, CancellationToken token)
        {
            var entity = new TblPack
            {
                Code = dto.Code,
                FullName = dto.FullName,
                NetVolume = dto.NetVolume,
                ExternalSize = dto.ExternalSize,
                PackWeight = dto.PackWeight,
                IsActive = dto.IsActive,
                UseForVacDl = dto.UseForVacDL,
                UseForOs = dto.UseForOS,
                EntryDate = DateTime.Now
            };

            _db.TblPacks.Add(entity);
            await _db.SaveChangesAsync(token);

            return entity.PackId;
        }

        // UPDATE
        public async Task<bool> Update(PackEditDto dto, CancellationToken token)
        {
            var entity = await _db.TblPacks.FirstOrDefaultAsync(x => x.PackId == dto.PackId, token);
            if (entity == null) return false;

            entity.Code = dto.Code;
            entity.FullName = dto.FullName;
            entity.NetVolume = dto.NetVolume;
            entity.ExternalSize = dto.ExternalSize;
            entity.PackWeight = dto.PackWeight;
            entity.IsActive = dto.IsActive;
            entity.UseForVacDl = dto.UseForVacDL;
            entity.UseForOs = dto.UseForOS;

            await _db.SaveChangesAsync(token);
            return true;
        }

        // PAGED LIST
        public async Task<PagingSearchOrderDto<PackGetDto>> GetPagedList(
            string? search, int page, int pageSize, string sortField, string sortOrder,
            CancellationToken token)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblPack> query = _db.TblPacks;

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.FullName.Contains(search) || x.Code.Contains(search));

            query = sortField switch
            {
                "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
                _ => query.OrderBy(x => x.PackId)
            };

            paging.TotalItems = await query.CountAsync(token);

            var list = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new PackGetDto
                {
                    PackId = x.PackId,
                    Code = x.Code,
                    FullName = x.FullName,
                    NetVolume = x.NetVolume,
                    ExternalSize = x.ExternalSize,
                    PackWeight = x.PackWeight,
                    IsActive = x.IsActive ?? false,
                    UseForVacDL = x.UseForVacDl.Value,
                    UseForOS = x.UseForOs.Value
                })
                .ToListAsync(token);

            return new PagingSearchOrderDto<PackGetDto>
            {
                Items = list,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }
    }

}
