#nullable disable

namespace VSSM.Infrastructure.Data.Models;

public partial class Menu
{
    // Primary Key
    public int Id { get; set; }

    // Menu Information
    public string Ttitle { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }
    public string Icon { get; set; }
    public int? MenuOrder { get; set; }

    // Hierarchy
    public int? ParentId { get; set; }

    // Navigation Properties
    public virtual Menu Parent { get; set; }

    public virtual ICollection<Menu> InverseParent { get; set; }
        = new List<Menu>();

    public virtual ICollection<RoleMenu> RoleMenus { get; set; }
        = new List<RoleMenu>();
}