using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Account;

public class LoginViewModel : ILoginViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین ۳ تا ۱۰۰ کاراکتر باشد")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "رمز عبور باید بین ۳ تا ۱۲۸ کاراکتر باشد")]
    public string Password { get; set; } = string.Empty;
}