namespace VSSM.Application.DTOs.BaseEntities.Recipient;

public class RecipientSearchDto
{
    public int RecipientId { get; set; }
    public string FullName { get; set; }
    public string Code { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
}
