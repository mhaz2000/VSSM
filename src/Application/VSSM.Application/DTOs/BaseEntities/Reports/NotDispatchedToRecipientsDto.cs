namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record NotDispatchedToRecipientsDto
    {
        public string FullName { get; set; }
        public int RecipientId { get; set; }
    }
}
