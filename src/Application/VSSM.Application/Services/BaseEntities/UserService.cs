using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.DTOs.BaseEntities.UserManagement;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class UserService(VSSMDbContext db) : IUserService
{
    public async Task<List<BaseInfoDto>> GetAdminUsersAsBaseInfoAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.TblUsers
            .Where(u => u.IsDeleted != true && u.IsAdmin == true)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new BaseInfoDto
            {
                Id = u.UserId,
                Title = ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UserNameExists(string userName, int? excludeUserId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return false;

        var name = userName.Trim();
        var query = db.TblUsers.Where(u => u.UserName == name && u.IsDeleted != true);

        if (excludeUserId.HasValue && excludeUserId.Value > 0)
            query = query.Where(u => u.UserId != excludeUserId.Value);

        return await query.AnyAsync(token);
    }

    public async Task<UserEditDto> GetById(int id, CancellationToken token)
    {
        var x = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == id, token);
        if (x == null) throw new Exception("کاربر یافت نشد");

        return new UserEditDto
        {
            UserId = x.UserId,
            UserName = x.UserName,
            Password = null,
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
            RoleId = x.RoleId
        };
    }

    public async Task<int> Create(UserCreateDto dto, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new Exception("نام کاربری الزامی است.");

        if (await UserNameExists(dto.UserName, null, token))
            throw new Exception("نام کاربری وارد شده تکراری است.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new Exception("کلمه عبور الزامی است.");

        ValidatePasswordPolicy(dto.Password, "کلمه عبور");

        var model = new TblUser
        {
            UserName = dto.UserName.Trim(),
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
            EntryDate = DateTime.Now,
            RoleId = dto.RoleId,
            MustChangePassword = true,
            IsDeleted = false
        };

        db.TblUsers.Add(model);
        await db.SaveChangesAsync(token);
        return model.UserId;
    }

    public async Task<bool> Update(UserEditDto dto, CancellationToken token)
    {
        var x = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == dto.UserId, token);
        if (x == null) throw new Exception("کاربر یافت نشد");

        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new Exception("نام کاربری الزامی است.");

        if (await UserNameExists(dto.UserName, dto.UserId, token))
            throw new Exception("نام کاربری وارد شده تکراری است.");

        if (!string.IsNullOrEmpty(dto.Password))
        {
            ValidatePasswordPolicy(dto.Password, "کلمه عبور");
            x.Password = PasswordHasher.Hash(dto.Password);
        }

        x.UserName = dto.UserName.Trim();
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
        x.IsDeleted = false;

        await db.SaveChangesAsync(token);
        return true;
    }

    private void ValidatePasswordPolicy(string password, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(password))
            return;

        var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$";
        if (!Regex.IsMatch(password, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100)))
        {
            throw new Exception($"{fieldName} باید حداقل ۸ کاراکتر و شامل حروف بزرگ، حروف کوچک، عدد و کاراکتر ویژه باشد.");
        }
    }

    public async Task<bool> ChangePassword(int userId, string newPassword, CancellationToken token)
    {
        ValidatePasswordPolicy(newPassword, "کلمه عبور جدید");

        var x = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if (x == null) throw new Exception("کاربر یافت نشد");

        x.Password = PasswordHasher.Hash(newPassword);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task SetMustChangePasswordAsync(int userId, bool value, CancellationToken token)
    {
        var x = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if (x == null) throw new Exception("کاربر یافت نشد");

        x.MustChangePassword = value;
        await db.SaveChangesAsync(token);
    }

    public async Task<bool> ToggleActiveness(int id, CancellationToken token)
    {
        var user = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == id, token);
        if (user == null) throw new Exception("کاربر یافت نشد");

        user.IsDeleted = !(user.IsDeleted ?? false);
        await db.SaveChangesAsync(token);

        return user.IsDeleted ?? false;
    }

    public async Task<PagingSearchOrderDto<UserGetDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken token)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        var query =
            from u in db.TblUsers
            where u.IsDeleted != true
            join r in db.TblRecipients on u.RecipientId equals r.RecipientId into gj
            from r in gj.DefaultIfEmpty()
            select new { User = u, Recipient = r };

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
                IsActive = !(x.User.IsDeleted ?? false),
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
        var x = await db.TblUsers.FirstOrDefaultAsync(u => u.UserName == username, token);
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
            RoleId = x.RoleId,
            CodeEdu = x.EduId,
            CodeJob = x.JobId,
            IsAdmin = x.IsAdmin ?? false,
            Course = x.Course,
            MustChangePassword = x.MustChangePassword
        };
    }

    public async Task<bool> VerifyPassword(int userId, string password, CancellationToken token)
    {
        var x = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if (x == null) return false;

        return PasswordHasher.Verify(password, x.Password);
    }

    public async Task UpgradeAllPlaintextPasswordsAsync(CancellationToken token)
    {
        var candidates = await db.TblUsers
            .Where(u => u.IsDeleted != true)
            .ToListAsync(token);

        var legacy = candidates
            .Where(u => u.Password != null && !PasswordHasher.LooksHashed(u.Password))
            .ToList();

        foreach (var u in legacy)
        {
            u.Password = PasswordHasher.Hash(u.Password!);
        }

        if (legacy.Count > 0)
            await db.SaveChangesAsync(token);
    }

    public async Task<bool> UpgradePasswordToHash(int userId, string plaintextPassword, CancellationToken token)
    {
        var x = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if (x == null) return false;

        x.Password = PasswordHasher.Hash(plaintextPassword);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<UserSearchResultDto> SearchUsersWithRecipientAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = from u in db.TblUsers
                    join r in db.TblRecipients on u.RecipientId equals r.RecipientId into recipientGroup
                    from r in recipientGroup.DefaultIfEmpty()
                    select new { User = u, Recipient = r };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var s = searchTerm.Trim();
            query = query.Where(x =>
                x.User.UserName.Contains(s) ||
                x.User.FirstName.Contains(s) ||
                x.User.LastName.Contains(s) ||
                (x.User.FirstName + " " + x.User.LastName).Contains(s)
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.User.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserWithRecipientDto
            {
                UserId = x.User.UserId,
                UserName = x.User.UserName,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                RecipientId = x.User.RecipientId,
                RecipientFullName = x.Recipient != null ? x.Recipient.FullName : null,
                RecipientCode = x.Recipient != null ? x.Recipient.Code : null,
                RecipientProvince = x.Recipient != null ? x.Recipient.Province : null,
                RecipientCity = x.Recipient != null ? x.Recipient.City : null,
                IsAdmin = x.User.IsAdmin ?? false
            })
            .ToListAsync(cancellationToken);

        return new UserSearchResultDto
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<bool> ChangeRecipientAsync(int userId, int newRecipientId, CancellationToken cancellationToken = default)
    {
        var user = await db.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user == null)
            throw new Exception("کاربر یافت نشد");

        var recipient = await db.TblRecipients.FirstOrDefaultAsync(r => r.RecipientId == newRecipientId, cancellationToken);
        if (recipient == null)
            throw new Exception("محل خدمت یافت نشد");

        if (recipient.IsActive == false)
            throw new Exception("محل خدمت انتخاب‌شده فعال نیست");

        user.RecipientId = newRecipientId;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<UserWithRecipientDto> GetUserWithRecipientAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = from u in db.TblUsers
                    join r in db.TblRecipients on u.RecipientId equals r.RecipientId into recipientGroup
                    from r in recipientGroup.DefaultIfEmpty()
                    where u.UserId == userId
                    select new UserWithRecipientDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        RecipientId = u.RecipientId,
                        RecipientFullName = r != null ? r.FullName : null,
                        RecipientCode = r != null ? r.Code : null,
                        RecipientProvince = r != null ? r.Province : null,
                        RecipientCity = r != null ? r.City : null,
                        IsAdmin = u.IsAdmin ?? false
                    };

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}