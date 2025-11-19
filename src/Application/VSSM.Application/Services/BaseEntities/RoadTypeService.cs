using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.DTOs.BaseEntities.RoadType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using ValidationException = FluentValidation.ValidationException;

namespace VSSM.Application.Services.BaseEntities
{
    public class RoadTypeService : IRoadTypeService
    {
        private readonly VSSMDbContext _dbContext;
        private readonly IValidator<RoadTypeCreateDto> _validator;
        private readonly IValidator<RoadTypeEditDto> _editValidator;

        public RoadTypeService(
            VSSMDbContext dbContext,
            IValidator<RoadTypeCreateDto> validator,
            IValidator<RoadTypeEditDto> editValidator)
        {
            _dbContext = dbContext;
            _validator = validator;
            _editValidator = editValidator;
        }

        public async Task<int> Create(RoadTypeCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(dto, cancellationToken);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            string sql = @"
                INSERT INTO tblRoadType (Code, FullName, IsActive)
                VALUES (@Code, @FullName, @IsActive);
            ";

            await _dbContext.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@Code", dto.Code ?? (object)DBNull.Value),
                new SqlParameter("@FullName", dto.FullName ?? (object)DBNull.Value),
             new SqlParameter("@IsActive", (object?)dto.IsActive ?? DBNull.Value)
             {
                 SqlDbType = System.Data.SqlDbType.Bit
             }
            );

            return 1;
        }

        public async Task<bool> Update(RoadTypeEditDto dto, CancellationToken cancellationToken)
        {
            var validation = await _editValidator.ValidateAsync(dto, cancellationToken);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            string sql = @"
                UPDATE tblRoadType
                SET Code = @Code,
                    FullName = @FullName,
                    IsActive = @IsActive
                WHERE RoadTypeID = @Id;
            ";

            int affected = await _dbContext.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@Id", dto.RoadTypeId),
                new SqlParameter("@Code", dto.Code ?? (object)DBNull.Value),
                new SqlParameter("@FullName", dto.FullName ?? (object)DBNull.Value),
            new SqlParameter("@IsActive", (object?)dto.IsActive ?? DBNull.Value)
            {
                SqlDbType = System.Data.SqlDbType.Bit
            }
            );

            return affected > 0;
        }

        public async Task<RoadTypeEditDto> GetById(int id, CancellationToken cancellationToken)
        {
            var sql = "SELECT * FROM tblRoadType WHERE RoadTypeID = @Id";

            var row = await _dbContext.TblRoadTypes
                .FromSqlRaw(sql, new SqlParameter("@Id", id))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                throw new Exception("Record not found!");

            return new RoadTypeEditDto
            {
                RoadTypeId = row.RoadTypeId,
                Code = row.Code,
                FullName = row.FullName,
                IsActive = row.IsActive ?? false
            };
        }


        public async Task<PagingSearchOrderDto<RoadTypeGetDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblRoadType> query = _dbContext.TblRoadTypes;

            // ---------- 1) SEARCH ----------
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    x.Code.Contains(search)
                );
            }

            // ---------- 2) SORTING ----------
            query = sortField switch
            {
                "FullName" => sortOrder == "asc"
                                ? query.OrderBy(x => x.FullName)
                                : query.OrderByDescending(x => x.FullName),

                "Code" => sortOrder == "asc"
                                ? query.OrderBy(x => x.Code)
                                : query.OrderByDescending(x => x.Code),

                _ => sortOrder == "asc"
                                ? query.OrderBy(x => x.RoadTypeId)
                                : query.OrderByDescending(x => x.RoadTypeId)
            };

            // ---------- 3) TOTAL ITEMS ----------
            paging.TotalItems = await query.CountAsync(cancellationToken);

            // ---------- 4) PAGE DATA ----------
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new RoadTypeGetDto
                {
                    RoadTypeId = x.RoadTypeId,
                    Code = x.Code,
                    FullName = x.FullName,
                    IsActive = x.IsActive ?? false
                })
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<RoadTypeGetDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }
    }
}
