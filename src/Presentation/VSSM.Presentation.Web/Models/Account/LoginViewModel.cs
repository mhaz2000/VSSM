using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VSSM.Presentation.Web.Models.Account;

public class LoginViewModel : ILoginViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین ۳ تا ۱۰۰ کاراکتر باشد")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "رمز عبور باید بین ۳ تا ۱۲۸ کاراکتر باشد")]
    public string Password { get; set; } = string.Empty;

    // [SECURITY]: کد امنیتی — در سمت سرور اعتبارسنجی می‌شود.
    public string Captcha { get; set; } = string.Empty;

    public string CaptchaToken { get; set; } = string.Empty;

    // فقط برای نمایش تولید می‌شود و نباید از فرم مدل‌بایند شود.
    [BindNever]
    public string CaptchaSvg { get; set; } = string.Empty;
}