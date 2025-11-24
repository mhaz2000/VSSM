using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.UserHome;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class UserHomeService : IUserHomeService
{
    private readonly VSSMDbContext _context;

    public UserHomeService(VSSMDbContext context)
    {
        _context = context;
    }

    public async Task<UserHomeDto> GetAsync()
    {
        var entity = await _context.TblUserHomes.FirstOrDefaultAsync();

        if (entity == null)
        {
            // اگر رکوردی نبود، یک رکورد خالی ذخیره می‌کنیم (Singleton Table)
            var sqlInsert = @"
INSERT INTO TblUserHome (Message, Message2, Message3)
VALUES ('', '', '')";

            await _context.Database.ExecuteSqlRawAsync(sqlInsert);

            return new UserHomeDto();
        }

        return new UserHomeDto
        {
            Message = entity.Message,
            Message2 = entity.Message2,
            Message3 = entity.Message3
        };
    }

    public async Task CreateOrUpdateAsync(UserHomeDto dto)
    {
        var sql = @"
IF EXISTS (SELECT 1 FROM TblUserHome)
    UPDATE TblUserHome
    SET 
        Message = {0},
        Message2 = {1},
        Message3 = {2}
ELSE
    INSERT INTO TblUserHome (Message, Message2, Message3)
    VALUES ({0}, {1}, {2})";

        await _context.Database.ExecuteSqlRawAsync(sql,
            dto.Message ?? "",
            dto.Message2 ?? "",
            dto.Message3 ?? "");
    }
}