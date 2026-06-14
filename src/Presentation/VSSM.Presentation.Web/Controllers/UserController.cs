using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.DTOs.BaseEntities.Dispatches;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Presentation.Web.Models.Base;

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
            int page = 1,
            int pageSize = 10,
            string sortField = "UserName",
            string sortOrder = "asc",
            CancellationToken token = default)
        {
            var list = await _userService.GetPagedList(
                search, page, pageSize, sortField, sortOrder, token);


            var model = new GridViewModel
            {
                SearchTerm = search,
                CurrentSort = sortField,
                CurrentDirection = sortField,
                Data = new PagedResult<object>
                {
                    Items = list.Items.Cast<object>().ToList(),
                    PageNumber = page,
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
                    Title = "چاپ پیش فاکتور",
                    Controller = "User",
                    Action = "Delete",
                    RouteIdProperty = nameof(UserGetDto.UserId),
                    LinkClass = "",
                    IconClass = "fa fa-trash",
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
                Password = dto.Password,
                Email = dto.Email,
                Phone1 = dto.Phone1,
                Phone2 = dto.Phone2,
                RecipientId = dto.RecipientId,
                PermissionLevel = dto.PermissionLevel,
                Idnumber = dto.Idnumber,
                CodeEdu = dto.CodeEdu,
                CodeJob = dto.CodeJob,
                IsAdmin = dto.IsAdmin,
                Course = dto.Course,
                CourseDate = PersianDateHelper.ToStringDate(dto.CourseDate),
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
                return View("New", model);



            if (model.UserId == null || model.UserId == 0)
            {
                // ======= CREATE =======
                var dto = new UserCreateDto
                {
                    UserName = model.UserName,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    RecipientId = model.RecipientId,
                    PermissionLevel = model.PermissionLevel.Value,
                    Phone1 = model.Phone1,
                    Phone2 = model.Phone2,
                    Idnumber = model.Idnumber,
                    CodeEdu = model.CodeEdu,
                    CodeJob = model.CodeJob,
                    IsAdmin = model.IsAdmin,
                    Course = model.Course,
                    CourseDate = PersianDateHelper.ToDateTime(model.CourseDate),
                    RoleId = model.RoleId
                };

                await _userService.Create(dto, token);
            }
            else
            {
                // ========= UPDATE =========
                var dto = new UserEditDto
                {
                    UserId = model.UserId.Value,
                    UserName = model.UserName,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    RecipientId = model.RecipientId,
                    PermissionLevel = model.PermissionLevel,
                    Phone1 = model.Phone1,
                    Phone2 = model.Phone2,
                    Idnumber = model.Idnumber,
                    CodeEdu = model.CodeEdu,
                    CodeJob = model.CodeJob,
                    IsAdmin = model.IsAdmin,
                    Course = model.Course,
                    CourseDate = PersianDateHelper.ToDateTime(model.CourseDate),
                    RoleId = model.RoleId
                };

                await _userService.Update(dto, token);
            }

            TempData["ToastMessage"] = "عملیات با موفقیت انجام شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        // ====================== DELETE ============================
        public async Task<IActionResult> Delete(int id, CancellationToken token)
        {
            await _userService.Delete(id, token);

            TempData["ToastMessage"] = "با موفقیت حذف شد";
            TempData["ToastType"] = "success";

            return RedirectToAction("Index");
        }

        // ====================== LOAD DROPDOWNS ============================
        private async Task LoadDropDowns(CancellationToken cancellationToken)
        {
            ViewBag.RecipientList = (await _recipientService.GetAll(cancellationToken))
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

            ViewBag.JobList = jobList is null ? [] : (await _baseInfoService.GetByParentIdAsync(60 ,cancellationToken))
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
