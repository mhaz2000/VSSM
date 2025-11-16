using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.JobType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class JobTypeService : IJobTypeService
    {
        private readonly VSSMDbContext _db;

        public JobTypeService(VSSMDbContext db)
        {
            _db = db;
        }

        public async Task<JobTypeEditDto> GetById(int id, CancellationToken token)
        {
            var entity = await _db.TblJobTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.JobTypeId == id, token);

            if (entity == null)
                throw new Exception("Record not found.");

            return new JobTypeEditDto
            {
                JobTypeId = entity.JobTypeId,
                FullName = entity.FullName,
                IsActive = entity.IsActive ?? false
            };
        }

        public async Task<int> Create(JobTypeCreateDto dto, CancellationToken token)
        {
            string sql = @"
                INSERT INTO TblJobType (FullName, IsActive)
                VALUES (@FullName, @IsActive)
            ";

            var p1 = new SqlParameter("@FullName", dto.FullName ?? (object)DBNull.Value);
            var p2 = new SqlParameter("@IsActive", dto.IsActive);

            await _db.Database.ExecuteSqlRawAsync(sql, p1, p2);

            return 1;
        }

        public async Task<bool> Update(JobTypeEditDto dto, CancellationToken token)
        {
            string sql = @"
                UPDATE TblJobType
                SET FullName=@FullName,
                    IsActive=@IsActive
                WHERE JobTypeId=@Id
            ";

            var p1 = new SqlParameter("@FullName", dto.FullName);
            var p2 = new SqlParameter("@IsActive", dto.IsActive);
            var p3 = new SqlParameter("@Id", dto.JobTypeId);

            await _db.Database.ExecuteSqlRawAsync(sql, p1, p2, p3);

            return true;
        }

        public async Task<PagingSearchOrderDto<JobTypeGetDto>> GetPagedList(
            string? search, int page, int pageSize,
            string sortField, string sortOrder, CancellationToken token)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblJobType> query = _db.TblJobTypes;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.FullName.Contains(search));
            }

            query = sortField switch
            {
                "FullName" =>
                    sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),

                _ =>
                    sortOrder == "asc" ? query.OrderBy(x => x.JobTypeId) : query.OrderByDescending(x => x.JobTypeId)
            };

            paging.TotalItems = await query.CountAsync(token);

            var rows = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new JobTypeGetDto
                {
                    JobTypeId = x.JobTypeId,
                    FullName = x.FullName,
                    IsActive = x.IsActive ?? false
                })
                .ToListAsync(token);

            return new PagingSearchOrderDto<JobTypeGetDto>
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
