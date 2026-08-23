#nullable disable

using System;
using System.Collections.Generic;

namespace VSSM.Infrastructure.Data.Models;

public partial class TblUser
{
    // Primary Key
    public int UserId { get; set; }

    // Authentication & Identity
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool MustChangePassword { get; set; } = false;

    // Personal Information
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Idnumber { get; set; }

    // Contact Information
    public string Phone1 { get; set; }
    public string Phone2 { get; set; }

    // Organization & Access
    public int? RecipientId { get; set; }
    public int? RoleId { get; set; }
    public bool? IsAdmin { get; set; }
    public bool? IsDeleted { get; set; }

    // Audit Information
    public DateTime? EntryDate { get; set; }
    public int? EnteredById { get; set; }
    public DateTime? Lastlogin { get; set; }

    // Additional Information
    public int? Storewarrning { get; set; }
    public int? EduId { get; set; }
    public int? JobId { get; set; }
    public string FreeNote { get; set; }
    public string Course { get; set; }
    public DateTime? CourseDate { get; set; }

    // Reference Navigation Properties
    public virtual TblBaseInfo CodeJobNavigation { get; set; }
    public virtual TblBaseInfo CodeEduNavigation { get; set; }
    public virtual TblUser EnteredBy { get; set; }
    public virtual Role Role { get; set; }

    // User Relations
    public virtual ICollection<TblUser> InverseEnteredBy { get; set; }
        = new List<TblUser>();

    public virtual ICollection<TblAdjustment> TblAdjustments { get; set; }
        = new List<TblAdjustment>();

    public virtual ICollection<TblDispatch> TblDispatches { get; set; }
        = new List<TblDispatch>();

    public virtual ICollection<TblRequest> TblRequests { get; set; }
        = new List<TblRequest>();
}