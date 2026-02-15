namespace VSSM.Presentation.Web.Models.Role
{
    public class RolePermissionViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<MenuCheckboxViewModel> Menus { get; set; }
    }

    public class MenuCheckboxViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int? ParentId { get; set; }
        public bool IsSelected { get; set; }
    }

}
