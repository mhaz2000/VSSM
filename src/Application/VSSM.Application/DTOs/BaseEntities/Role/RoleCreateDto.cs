using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.Role;

public class RoleCreateDto
{
    [Required(ErrorMessage = "نام نقش الزامی است.")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "نام نقش باید بین ۲ تا ۵۰ کاراکتر باشد.")]
    public string Name { get; set; }
}
