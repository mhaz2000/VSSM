using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.User;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

public class UserService : IUserService
{
    private readonly VSSMDbContext _db;

    public UserService(VSSMDbContext db)
    {
        _db = db;
    }

    // ---------------- GET BY ID ----------------
    public async Task<UserEditDto> GetById(int id, CancellationToken token)
    {
        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserId == id, token);
        if (x == null) throw new Exception("User not found");

        return new UserEditDto
        {
            UserId = x.UserId,
            UserName = x.UserName,
            Password = x.Password,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Idnumber = x.Idnumber,
            Phone1 = x.Phone1,
            Phone2 = x.Phone2,
            RecipientId = x.RecipientId,
            PermissionLevel = x.PermissionLevel,
            CodeEdu = x.CodeEdu,
            CodeJob = x.CodeJob,
            IsAdmin = x.IsAdmin ?? false,
            Course = x.Course,
            CourseDate = x.CourseDate,
            RoleId = x.RoleId
        };
    }

    // ---------------- CREATE ----------------
    public async Task<int> Create(UserCreateDto dto, CancellationToken token)
    {
        var model = new TblUser
        {
            UserName = dto.UserName,
            Password = dto.Password,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone1 = dto.Phone1,
            Phone2 = dto.Phone2,
            Idnumber = dto.Idnumber,
            RecipientId = dto.RecipientId,
            PermissionLevel = dto.PermissionLevel,
            CodeEdu = dto.CodeEdu,
            CodeJob = dto.CodeJob,
            IsAdmin = dto.IsAdmin,
            Course = dto.Course,
            CourseDate = dto.CourseDate,
            EntryDate = DateTime.Now,
            RoleId = dto.RoleId
        };

        _db.TblUsers.Add(model);
        await _db.SaveChangesAsync(token);
        return model.UserId;
    }

    // ---------------- UPDATE ----------------
    public async Task<bool> Update(UserEditDto dto, CancellationToken token)
    {
        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserId == dto.UserId, token);
        if (x == null) throw new Exception("User not found");

        x.UserName = dto.UserName;
        x.Password = dto.Password;
        x.FirstName = dto.FirstName;
        x.LastName = dto.LastName;
        x.Email = dto.Email;
        x.Phone1 = dto.Phone1;
        x.Phone2 = dto.Phone2;
        x.Idnumber = dto.Idnumber;
        x.RecipientId = dto.RecipientId;
        x.PermissionLevel = dto.PermissionLevel;
        x.RoleId = dto.RoleId;
        x.CodeEdu = dto.CodeEdu;
        x.CodeJob = dto.CodeJob;
        x.IsAdmin = dto.IsAdmin;
        x.Course = dto.Course;
        x.CourseDate = dto.CourseDate;

        await _db.SaveChangesAsync(token);
        return true;
    }

    // ---------------- SOFT DELETE ----------------
    public async Task<bool> Delete(int id, CancellationToken token)
    {
        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserId == id, token);
        if (x == null) throw new Exception("User not found");

        x.IsDeleted = true;
        await _db.SaveChangesAsync(token);

        return true;
    }

    // ---------------- PAGED LIST ----------------
    public async Task<PagingSearchOrderDto<UserGetDto>> GetPagedList(
     string? search,
     int page,
     int pageSize,
     string sortField,
     string sortOrder,
     CancellationToken token)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        // Base query (not deleted users)
        var query =
            from u in _db.TblUsers
            join r in _db.TblRecipients on u.RecipientId equals r.RecipientId into gj
            from r in gj.DefaultIfEmpty()   // LEFT JOIN
            where u.IsDeleted != true
            select new { User = u, Recipient = r };

        // ============= SEARCH =============
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();

            query = query.Where(x =>
                x.User.FirstName.Contains(s) ||
                x.User.LastName.Contains(s) ||
                x.User.UserName.Contains(s) ||
                (x.Recipient != null && x.Recipient.FullName.Contains(s))
            );
        }

        // ============= SORT =============
        query = sortField switch
        {
            "UserName" => sortOrder == "asc"
                ? query.OrderBy(x => x.User.UserName)
                : query.OrderByDescending(x => x.User.UserName),

            "FirstName" => sortOrder == "asc"
                ? query.OrderBy(x => x.User.FirstName)
                : query.OrderByDescending(x => x.User.FirstName),

            "LastName" => sortOrder == "asc"
                ? query.OrderBy(x => x.User.LastName)
                : query.OrderByDescending(x => x.User.LastName),

            _ => query.OrderBy(x => x.User.UserId)
        };

        paging.TotalItems = await query.CountAsync(token);

        // ============= SELECT DTO =============
        var list = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new UserGetDto
            {
                UserId = x.User.UserId,
                UserName = x.User.UserName,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Phone1 = x.User.Phone1,
                Phone2 = x.User.Phone2,
                JobName = x.User.CodeJobNavigation.FullName,
                RecipientName = x.Recipient != null ? x.Recipient.FullName : ""
            })
            .ToListAsync(token);

        return new PagingSearchOrderDto<UserGetDto>
        {
            Items = list,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<UserEditDto> GetByUsername(string username, CancellationToken token)
    {
        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserName == username, token);
        if (x == null) return null;

        return new UserEditDto
        {
            UserId = x.UserId,
            UserName = x.UserName,
            Password = x.Password,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Idnumber = x.Idnumber,
            Phone1 = x.Phone1,
            Phone2 = x.Phone2,
            RecipientId = x.RecipientId,
            PermissionLevel = x.PermissionLevel,
            RoleId = x.RoleId,
            CodeEdu = x.CodeEdu,
            CodeJob = x.CodeJob,
            IsAdmin = x.IsAdmin ?? false,
            Course = x.Course,
            CourseDate = x.CourseDate
        };
    }
}
