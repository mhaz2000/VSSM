using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.User;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;
using System.Text.RegularExpressions;

namespace VSSM.Application.Services.BaseEntities;

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
            Password = null, // Never surface the stored hash to the UI; edit form is blank-by-design.
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Idnumber = x.Idnumber,
            Phone1 = x.Phone1,
            Phone2 = x.Phone2,
            RecipientId = x.RecipientId,
            CodeEdu = x.EduId,
            CodeJob = x.JobId,
            IsAdmin = x.IsAdmin ?? false,
            Course = x.Course,
            CourseDate = x.CourseDate,
            RoleId = x.RoleId
        };
    }

    // ---------------- CREATE ----------------
    public async Task<int> Create(UserCreateDto dto, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new Exception("کلمه عبور الزامی است.");

        ValidatePasswordPolicy(dto.Password, "کلمه عبور");

        var model = new TblUser
        {
            UserName = dto.UserName,
            Password = PasswordHasher.Hash(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone1 = dto.Phone1,
            Phone2 = dto.Phone2,
            Idnumber = dto.Idnumber,
            RecipientId = dto.RecipientId,
            EduId = dto.CodeEdu,
            JobId = dto.CodeJob,
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

        // Only validate/hash password if it's provided — a blank password on edit
        // means "keep current", so we leave the stored hash untouched in that case.
        if (!string.IsNullOrEmpty(dto.Password))
        {
            ValidatePasswordPolicy(dto.Password, "کلمه عبور");
            x.Password = PasswordHasher.Hash(dto.Password);
        }

        x.UserName = dto.UserName;
        x.FirstName = dto.FirstName;
        x.LastName = dto.LastName;
        x.Email = dto.Email;
        x.Phone1 = dto.Phone1;
        x.Phone2 = dto.Phone2;
        x.Idnumber = dto.Idnumber;
        x.RecipientId = dto.RecipientId;
        x.RoleId = dto.RoleId;
        x.EduId = dto.CodeEdu;
        x.JobId = dto.CodeJob;
        x.IsAdmin = dto.IsAdmin;
        x.Course = dto.Course;
        x.CourseDate = dto.CourseDate;

        await _db.SaveChangesAsync(token);
        return true;
    }

    // ---------------- VALIDATE PASSWORD POLICY ----------------
    private void ValidatePasswordPolicy(string password, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(password))
            return;

        var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$";
        if (!Regex.IsMatch(password, pattern))
        {
            throw new Exception($"{fieldName} باید حداقل ۸ کاراکتر و شامل حروف بزرگ، حروف کوچک، عدد و کاراکتر ویژه باشد.");
        }
    }

    public async Task<bool> ChangePassword(int userId, string newPassword, CancellationToken token)
    {
        ValidatePasswordPolicy(newPassword, "کلمه عبور جدید");

        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if (x == null) throw new Exception("User not found");

        x.Password = PasswordHasher.Hash(newPassword);
        await _db.SaveChangesAsync(token);
        return true;
    }

    // ---------------- SOFT DELETE ----------------
    public async Task<bool> ToggleActiveness(int id, CancellationToken token)
    {
        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserId == id, token);
        if (x == null) throw new Exception("User not found");

        x.IsDeleted = !x.IsDeleted;
        await _db.SaveChangesAsync(token);

        return x.IsDeleted.Value;
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
                IsActive = x.User.IsDeleted ?? false,
                JobName = x.User.CodeJobNavigation.Title,
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
            // The stored hash is returned here for the login path to verify against and to
            // detect legacy plaintext values for transparent upgrade. This is an internal
            // DTO — it never reaches the browser (the edit form uses GetById, which omits it).
            Password = x.Password,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Idnumber = x.Idnumber,
            Phone1 = x.Phone1,
            Phone2 = x.Phone2,
            RecipientId = x.RecipientId,
            RoleId = x.RoleId,
            CodeEdu = x.EduId,
            CodeJob = x.JobId,
            IsAdmin = x.IsAdmin ?? false,
            Course = x.Course,
            CourseDate = x.CourseDate
        };
    }

    // ---------------- VERIFY PASSWORD ----------------
    // True when the plaintext password matches the stored PBKDF2 hash.
    // False for a stored plaintext value (handled by the caller's upgrade path).
    public async Task<bool> VerifyPassword(int userId, string password, CancellationToken token)
    {
        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if (x == null) return false;

        return PasswordHasher.Verify(password, x.Password);
    }

    // ---------------- UPGRADE PLAINTEXT TO HASH ----------------
    // Re-hashes and persists a password that was stored as plaintext. Used by the login
    // path to migrate legacy users transparently on their first successful login.
    public async Task<bool> UpgradePasswordToHash(int userId, string plaintextPassword, CancellationToken token)
    {
        var x = await _db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if (x == null) return false;

        x.Password = PasswordHasher.Hash(plaintextPassword);
        await _db.SaveChangesAsync(token);
        return true;
    }
}
