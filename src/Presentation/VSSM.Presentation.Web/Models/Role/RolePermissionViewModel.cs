using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Role;

public class RolePermissionViewModel
{
    public int RoleId { get; set; }
    [Required(ErrorMessage = "وارد کردن نام نقش الزامی است")]
    [Display(Name = "نام نقش")]
    public string RoleName { get; set; } = string.Empty;
    public List<MenuCheckboxViewModel> Menus { get; set; } = [];
}

public class MenuCheckboxViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int? ParentId { get; set; }
    public bool IsSelected { get; set; }
}
