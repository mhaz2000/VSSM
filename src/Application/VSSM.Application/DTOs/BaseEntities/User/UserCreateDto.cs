using Microsoft.EntityFrameworkCore.Metadata;

public class UserCreateDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string Email { get; set; }
    public int? RecipientId { get; set; }
    public int? RoleId { get; set; }

    public string Phone1 { get; set; }
    public string Phone2 { get; set; }

    public string Idnumber { get; set; }
    public int? CodeEdu { get; set; }
    public int? CodeJob { get; set; }
    public bool IsAdmin { get; set; }

    // [SECURITY - A07]: surfaced so the login path (GetByUsername -> UserEditDto) can detect
    // that the user must change their password on first login.
    public bool MustChangePassword { get; set; }

    public string Course { get; set; }
    public DateTime? CourseDate { get; set; }

    public string FullName => FirstName + " " + LastName; 
}
