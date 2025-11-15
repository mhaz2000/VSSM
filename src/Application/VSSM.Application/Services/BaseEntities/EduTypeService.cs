using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.EduType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class EduTypeService : IEduTypeService
    {
        private readonly VSSMDbContext _db;

        public EduTypeService(VSSMDbContext db)
        {
            _db = db;
        }

        public async Task<EduTypeEditDto> GetById(int id, CancellationToken token)
        {
            var entity = await _db.TblEduTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EduTypeId == id, token);

            if (entity == null)
                throw new Exception("Record not found.");

            return new EduTypeEditDto
            {
                EduTypeId = entity.EduTypeId,
                FullName = entity.FullName,
                IsActive = entity.IsActive ?? false
            };
        }

        public async Task<int> Create(EduTypeCreateDto dto, CancellationToken token)
        {
            const string sql = @"
                INSERT INTO TblEduType (FullName, IsActive)
                VALUES (@FullName, @IsActive)
            ";

            var p1 = new SqlParameter("@FullName", dto.FullName ?? (object)DBNull.Value);
            var p2 = new SqlParameter("@IsActive", dto.IsActive);

            await _db.Database.ExecuteSqlRawAsync(sql, p1, p2);

            return 1;
        }

        public async Task<bool> Update(EduTypeEditDto dto, CancellationToken token)
        {
            const string sql = @"
                UPDATE TblEduType
                SET FullName = @FullName,
                    IsActive = @IsActive
                WHERE EduTypeId = @Id
            ";

            var p1 = new SqlParameter("@FullName", dto.FullName);
            var p2 = new SqlParameter("@IsActive", dto.IsActive);
            var p3 = new SqlParameter("@Id", dto.EduTypeId);

            await _db.Database.ExecuteSqlRawAsync(sql, p1, p2, p3);

            return true;
        }

        public async Task<PagingSearchOrderDto<EduTypeGetDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken token)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblEduType> query = _db.TblEduTypes;

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.FullName.Contains(search));
            }

            // Sorting
            query = sortField switch
            {
                "Name" or "FullName" =>
                    sortOrder == "asc"
                        ? query.OrderBy(x => x.FullName)
                        : query.OrderByDescending(x => x.FullName),

                _ => sortOrder == "asc"
                        ? query.OrderBy(x => x.EduTypeId)
                        : query.OrderByDescending(x => x.EduTypeId)
            };

            paging.TotalItems = await query.CountAsync(token);

            var rows = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new EduTypeGetDto
                {
                    EduTypeId = x.EduTypeId,
                    FullName = x.FullName,
                    IsActive = x.IsActive ?? false
                })
                .ToListAsync(token);

            return new PagingSearchOrderDto<EduTypeGetDto>
            {
                Items = rows,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }
    }
}
