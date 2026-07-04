using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;
using VSSM.Presentation.Web.Models.User;

namespace VSSM.Presentation.Web.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IRecipientService _recipientService;
        private readonly IRoleService _roleService;
        private readonly IBaseInfoService _baseInfoService;

        public UserController(
            IUserService userService,
            IRecipientService recipientService,
            IBaseInfoService baseInfoService,
            IRoleService roleService)
        {
            _userService = userService;
            _recipientService = recipientService;
            _roleService = roleService;
            _baseInfoService = baseInfoService;
        }

        // ====================== INDEX ============================
        public async Task<IActionResult> Index(
            string? search,
            int pageNumber = 1,
            int pageSize = 10,
            string sortField = "UserName",
            string sortOrder = "asc",
            CancellationToken token = default)
        {
            var list = await _userService.GetPagedList(
                search, pageNumber, pageSize, sortField, sortOrder, token);


            var model = new GridViewModel
            {
                SearchTerm = search,
                CurrentSort = sortField,
                CurrentDirection = sortField,
                Data = new PagedResult<object>
                {
                    Items = list.Items.Cast<object>().ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = list.Paging.TotalPages
                },
                Columns = new List<GridColumn>
            {
                new() { Header = "نام کاربری", PropertyName = nameof(UserGetDto.UserName),  Sortable = false },
                new() { Header = "نام", PropertyName = nameof(UserGetDto.FirstName), Sortable = false },
                new() { Header = "نام خانوادگی", PropertyName = nameof(UserGetDto.LastName), Sortable = false },
                new() { Header = "تلفن", PropertyName = nameof(UserGetDto.Phone1),  Sortable = false },
                new() { Header = "گیرنده", PropertyName = nameof(UserGetDto.RecipientName),  Sortable = false },
                new() { Header = "شغل", PropertyName = nameof(UserGetDto.JobName),  Sortable = false },
                new() { Header = "فعال بودن", PropertyName = nameof(UserGetDto.IsActiveText),  Sortable = false },
                new() { Header = "عملیات", IsActionColumn = true}
            },
                Actions = new List<GridAction>
            {
                new()
                {
                    Title = "ویرایش",
                    Controller = "User",
                    Action = "New",
                    RouteIdProperty = nameof(UserGetDto.UserId),
                    LinkClass = "",
                    IconClass = "fa fa-edit",
                },
                new()
                {
                    Title = "تغییر رمز عبور",
                    Controller = "User",
                    Action = "ChangePassword",
                    RouteIdProperty = nameof(UserGetDto.UserId),
                    LinkClass = "",
                    IconClass = "fa fa-key",
                    OpenModal = true
                },
                new()
                {
                    Title = "حذف",
                    Controller = "User",
                    Action = "ToggleActiveness",
                    RouteIdProperty = nameof(UserGetDto.UserId),
                    LinkClass = "delete-user",
                    IconClass = "fa fa-trash",
                    Method = "Post",
                    DynamicSkip = item =>
                    {
                         var row = (UserGetDto)item;

                        return (!row.IsActive).ToString();
                    }
                },
                new()
                {
                    Title = "فعال کردن",
                    Controller = "User",
                    Action = "ToggleActiveness",
                    RouteIdProperty = nameof(UserGetDto.UserId),
                    LinkClass = "delete-user",
                    IconClass = "fa fa-check",
                    Method = "Post",
                    DynamicSkip = item =>
                    {
                         var row = (UserGetDto)item;

                        return row.IsActive.ToString();
                    }
                }
            }
            };


            return View(model);
        }

        // ====================== NEW / EDIT ============================
        public async Task<IActionResult> New(int? id, CancellationToken token)
        {
            await LoadDropDowns(token);

            if (id == null)
                return View(new UserViewModel());

            // Load existing user
            var dto = await _userService.GetById(id.Value, token);
            if (dto == null)
                return NotFound();

            var model = new UserViewModel
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone1 = dto.Phone1,
                Phone2 = dto.Phone2,
                RecipientId = dto.RecipientId,
                Idnumber = dto.Idnumber,
                CodeEdu = dto.CodeEdu,
                CodeJob = dto.CodeJob,
                IsAdmin = dto.IsAdmin,
                Course = dto.Course,
                RoleId = dto.RoleId
            };

            return View(model);
        }

        // ====================== CREATE/UPDATE ============================
        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model, CancellationToken token)
        {
            await LoadDropDowns(token);

            if (!ModelState.IsValid)
            {
                TempData["ToastMessage"] = "تمام فیلدهای اجباری باید پر شوند";
                TempData["ToastType"] = "error";
                return View("New", model);
            }


            if (model.UserId == null || model.UserId == 0)
            {

                var dto = new UserCreateDto
                {
                    UserName = model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    RecipientId = model.RecipientId,
                    Phone1 = model.Phone1,
                    Phone2 = model.Phone2,
                    Idnumber = model.Idnumber,
                    CodeEdu = model.CodeEdu,
                    CodeJob = model.CodeJob,
                    IsAdmin = model.IsAdmin,
                    Course = model.Course,
                    RoleId = model.RoleId
                };

                await _userService.Create(dto, token);
            }
            else
            {
                // ========= UPDATE =========
                // For updates, only validate password if it's provided (not null/empty)
                // This allows editing other user fields without changing the password
                var dto = new UserEditDto
                {
                    UserId = model.UserId.Value,
                    UserName = model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    RecipientId = model.RecipientId,
                    Phone1 = model.Phone1,
                    Phone2 = model.Phone2,
                    Idnumber = model.Idnumber,
                    CodeEdu = model.CodeEdu,
                    CodeJob = model.CodeJob,
                    IsAdmin = model.IsAdmin,
                    Course = model.Course,
                    RoleId = model.RoleId
                };

                await _userService.Update(dto, token);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        // ====================== DELETE ============================
        [HttpPost]
        public async Task<IActionResult> ToggleActiveness(int id, CancellationToken token)
        {
            var active = await _userService.ToggleActiveness(id, token);

            TempData["ToastMessage"] = active ? "با موفقیت فعال شد." : "با موفقیت حذف شد.";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        // ====================== CHANGE PASSWORD ============================
        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            var model = new ChangePasswordViewModel { UserId = id };
            return PartialView("_ChangePassword", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return PartialView("_ChangePassword", model);

            await _userService.ChangePassword(model.UserId, model.NewPassword, token);

            TempData["ToastMessage"] = "رمز عبور با موفقیت تغییر یافت";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        // ====================== LOAD DROPDOWNS ============================
        private async Task LoadDropDowns(CancellationToken cancellationToken)
        {
            ViewBag.RecipientList = (await _recipientService.GetActiveRecipients(cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.RecipientId.ToString(),
                    Text = x.FullName
                })
                .ToList();


            var eduList = await _baseInfoService.GetByParentIdAsync(61, cancellationToken);
            ViewBag.EduList = eduList is null ? [] : (await _baseInfoService.GetByParentIdAsync(61, cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                })
                .ToList();

            var jobList = await _baseInfoService.GetByParentIdAsync(60, cancellationToken);

            ViewBag.JobList = jobList is null ? [] : (await _baseInfoService.GetByParentIdAsync(60, cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Title
                })
                .ToList();

            ViewBag.PermissionList = Enum.GetValues(typeof(PermissionLevelEnum))
                .Cast<PermissionLevelEnum>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = x.GetDisplayName() // Persian name
                })
                .ToList();


            ViewBag.Roles = (await _roleService.GetAll(cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToList();
        }

    }
}
