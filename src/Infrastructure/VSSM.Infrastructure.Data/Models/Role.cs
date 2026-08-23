#nullable disable

namespace VSSM.Infrastructure.Data.Models;

public partial class Role
{
    // Primary Key
    public int Id { get; set; }

    // Role Information
    public string Name { get; set; }

    // Relations
    public virtual ICollection<RoleMenu> RoleMenus { get; set; }
        = new List<RoleMenu>();

    public virtual ICollection<TblUser> TblUsers { get; set; }
        = new List<TblUser>();
}