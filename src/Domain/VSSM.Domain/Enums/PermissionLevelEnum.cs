using System.ComponentModel.DataAnnotations;

public enum PermissionLevelEnum
{
    [Display(Name = "هیچ دسترسی")]
    None = 0,

    [Display(Name = "گزارشات")]
    Reports = 1,

    [Display(Name = "ورود محموله (Arrivals)")]
    Arrivals = 2,

    [Display(Name = "خروج محموله (Dispatches)")]
    Dispatches = 3,

    [Display(Name = "اطلاعات پایه")]
    BasicData = 4,

    [Display(Name = "مدیر (Adjustment)")]
    Admin = 5
}
