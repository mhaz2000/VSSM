#nullable disable

namespace VSSM.Infrastructure.Data.Models;

public partial class RoleMenu
{
    // Primary Key
    public int Id { get; set; }

    // Foreign Keys
    public int RoleId { get; set; }
    public int MenuId { get; set; }

    // Navigation Properties
    public virtual Menu Menu { get; set; }
    public virtual Role Role { get; set; }
}