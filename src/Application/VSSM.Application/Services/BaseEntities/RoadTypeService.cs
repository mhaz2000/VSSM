using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

        // ---------------------------------------------------------
        // CREATE (RAW SQL)
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // UPDATE (RAW SQL)
        // ---------------------------------------------------------
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

        // ---------------------------------------------------------
        // GET ALL  (RAW SQL → TblRoadType)
        // ---------------------------------------------------------
        public async Task<List<RoadTypeGetDto>> GetAll(CancellationToken cancellationToken)
        {
            var sql = "SELECT * FROM tblRoadType";

            var items = await _dbContext.TblRoadTypes
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return items.Select(x => new RoadTypeGetDto
            {
                RoadTypeId = x.RoadTypeId,
                Code = x.Code,
                FullName = x.FullName,
                IsActive = x.IsActive ?? false
            }).ToList();
        }

        // ---------------------------------------------------------
        // GET BY ID (RAW SQL → TblRoadType)
        // ---------------------------------------------------------
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
    }
}
