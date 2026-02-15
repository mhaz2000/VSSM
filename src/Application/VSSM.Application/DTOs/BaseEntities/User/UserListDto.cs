namespace VSSM.Application.DTOs.BaseEntities.User
{
    public class UserListDto
    {
        public int UserId { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }   // FirstName + LastName
        public string RecipientName { get; set; }

        public int? PermissionLevel { get; set; }
        public int? RoleId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
