namespace VSSM.Application.DTOs.BaseEntities.UserManagement;

public class UserSearchResultDto
{
    public List<UserWithRecipientDto> Items { get; set; }
    public int TotalCount { get; set; }
}
