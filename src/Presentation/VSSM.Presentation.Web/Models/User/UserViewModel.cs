using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.User;

public class UserViewModel
{
    public int? UserId { get; set; }

    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [StringLength(50, ErrorMessage = "نام کاربری حداکثر ۵۰ کاراکتر است.")]
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "گذرواژه")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "گذرواژه و تکرار آن مطابقت ندارند.")]
    [Display(Name = "تکرار گذرواژه")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "نام الزامی است.")]
    [StringLength(100, ErrorMessage = "نام حداکثر ۱۰۰ کاراکتر است.")]
    [Display(Name = "نام")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [StringLength(100, ErrorMessage = "نام خانوادگی حداکثر ۱۰۰ کاراکتر است.")]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; }

    [EmailAddress(ErrorMessage = "فرمت ایمیل معتبر نیست.")]
    [StringLength(150, ErrorMessage = "ایمیل حداکثر ۱۵۰ کاراکتر است.")]
    [Display(Name = "ایمیل")]
    public string? Email { get; set; }

    [RegularExpression(@"^\d+$", ErrorMessage = "تلفن باید فقط شامل رقم باشد.")]
    [StringLength(15, ErrorMessage = "تلفن حداکثر ۱۵ رقم است.")]
    [Display(Name = "تلفن")]
    public string? Phone1 { get; set; }

    [RegularExpression(@"^\d+$", ErrorMessage = "فکس باید فقط شامل رقم باشد.")]
    [StringLength(15, ErrorMessage = "فکس حداکثر ۱۵ رقم است.")]
    [Display(Name = "فکس")]
    public string? Phone2 { get; set; }

    [StringLength(10, ErrorMessage = "شماره شناسنامه حداکثر ۱۰ کاراکتر است.")]
    [Display(Name = "شماره شناسنامه")]
    public string? Idnumber { get; set; }

    [Required(ErrorMessage = "گیرنده الزامی است.")]
    [Range(1, int.MaxValue, ErrorMessage = "گیرنده الزامی است.")]
    [Display(Name = "گیرنده")]
    public int? RecipientId { get; set; }

    [Display(Name = "تحصیلات")]
    public int? CodeEdu { get; set; }

    [Display(Name = "شغل")]
    public int? CodeJob { get; set; }

    [Required(ErrorMessage = "نقش الزامی است.")]
    [Range(1, int.MaxValue, ErrorMessage = "نقش الزامی است.")]
    [Display(Name = "نقش")]
    public int? RoleId { get; set; }

    [StringLength(200, ErrorMessage = "دوره آموزشی حداکثر ۲۰۰ کاراکتر است.")]
    [Display(Name = "دوره آموزشی")]
    public string? Course { get; set; }

    [Display(Name = "مدیر سیستم")]
    public bool IsAdmin { get; set; }
}