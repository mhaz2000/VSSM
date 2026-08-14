namespace VSSM.Application.DTOs.BaseEntities.UserManagement;

public class UserWithRecipientDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } 
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? RecipientId { get; set; }
    public string RecipientFullName { get; set; }
    public string RecipientCode { get; set; }
    public string RecipientProvince { get; set; }
    public string RecipientCity { get; set; }
    public bool IsAdmin { get; set; }
}
