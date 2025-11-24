using System.ComponentModel.DataAnnotations;
using VSSM.Presentation.Web.Models.Utility;

public class UserViewModel
{
    public int? UserId { get; set; }

    [Required(ErrorMessage = "نام کاربری نمی‌تواند خالی باشد.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "کلمه عبور نمی‌تواند خالی باشد.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "نام نمی‌تواند خالی باشد.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "نام خانوادگی نمی‌تواند خالی باشد.")]
    public string LastName { get; set; }

    public string? Email { get; set; } = null;

 
    public int? RecipientId { get; set; }

    [Required(ErrorMessage = "سطح دسترسی نمی‌تواند خالی باشد.")]
    public int? PermissionLevel { get; set; }

    public string? Phone1 { get; set; }
    public string? Phone2 { get; set; }

    public string? Idnumber { get; set; }

 
    public int? CodeEdu { get; set; }

 
    public int? CodeJob { get; set; }

    public bool IsAdmin { get; set; }

    public string Course { get; set; }
    // 👇 این را اضافه کن
    [Display(Name = "تاریخ ثبت")]
    [RegularExpression(@"^\d{2}/\d{2}/\d{2}$",
        ErrorMessage = "فرمت تاریخ باید به صورت yy/MM/dd باشد (مثال: 03/01/24).")]
    public string CourseDate { get; set; }
}
