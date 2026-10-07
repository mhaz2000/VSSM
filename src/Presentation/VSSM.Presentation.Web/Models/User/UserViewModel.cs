using System.ComponentModel.DataAnnotations;
using VSSM.Presentation.Web.Models.Utility;

public class UserViewModel
{
    public int? UserId { get; set; }

    [Required(ErrorMessage = "نام کاربری نمی‌تواند خالی باشد.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "نام نمی‌تواند خالی باشد.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "نام خانوادگی نمی‌تواند خالی باشد.")]
    public string LastName { get; set; }

    public string? Email { get; set; } = null;

 
    public int? RecipientId { get; set; }

    public int? RoleId { get; set; }

    public string? Phone1 { get; set; }
    public string? Phone2 { get; set; }

    public string? Idnumber { get; set; }

 
    public int? CodeEdu { get; set; }

 
    public int? CodeJob { get; set; }

    public bool IsAdmin { get; set; }

    public string? Course { get; set; }

    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }


}
