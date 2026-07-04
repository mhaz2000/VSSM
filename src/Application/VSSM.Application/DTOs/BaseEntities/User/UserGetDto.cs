public class UserGetDto
{
    public int UserId { get; set; }

    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string Phone1 { get; set; }
    public string Phone2 { get; set; }

    public string RecipientName { get; set; }
    public string JobName { get; set; }
    public bool IsActive { get; internal set; }
    public string IsActiveText => IsActive ? "فعال" : "غیر فعال";
}
