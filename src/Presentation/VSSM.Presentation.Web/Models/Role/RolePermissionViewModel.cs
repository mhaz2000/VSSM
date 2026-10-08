using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Role;

public class RolePermissionViewModel
{
    public int? RoleId { get; set; }

    [Required(ErrorMessage = "نام نقش الزامی است.")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "نام نقش باید بین ۲ تا ۵۰ کاراکتر باشد.")]
    public string RoleName { get; set; }

    public List<MenuCheckboxViewModel> Menus { get; set; } = [];
}

public class MenuCheckboxViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int? ParentId { get; set; }
    public bool IsSelected { get; set; }
}
