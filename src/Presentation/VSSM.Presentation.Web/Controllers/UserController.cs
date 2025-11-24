using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Common.Helper;
using VSSM.Presentation.Web.Models.Utility;

namespace VSSM.Presentation.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRecipientService _recipientService;
        private readonly IEduTypeService _eduService;
        private readonly IJobTypeService _jobService;

        public UserController(
            IUserService userService,
            IRecipientService recipientService,
            IEduTypeService eduService,
            IJobTypeService jobService)
        {
            _userService = userService;
            _recipientService = recipientService;
            _eduService = eduService;
            _jobService = jobService;
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

            return View(list);
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

            ViewBag.EduList = (await _eduService.GetAll(cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.EduTypeId.ToString(),
                    Text = x.FullName
                })
                .ToList();

            ViewBag.JobList = (await _jobService.GetAll(cancellationToken))
                .Select(x => new SelectListItem
                {
                    Value = x.JobTypeId.ToString(),
                    Text = x.FullName
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
        }

    }
}
