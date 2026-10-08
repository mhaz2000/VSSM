using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.User;

public class ChangePasswordViewModel
{
    public int UserId { get; set; }

    [StringLength(64, MinimumLength = 8,
        ErrorMessage = "کلمه عبور باید بین ۸ تا ۶۴ کاراکتر باشد.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$",
        ErrorMessage = "کلمه عبور باید شامل حروف بزرگ، حروف کوچک، عدد و کاراکتر ویژه باشد."
    )]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "تکرار کلمه عبور نمی‌تواند خالی باشد.")]
    [Compare(nameof(NewPassword), ErrorMessage = "کلمه عبور و تکرار آن یکسان نیست.")]
    public string ConfirmPassword { get; set; }
}
