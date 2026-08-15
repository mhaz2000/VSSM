namespace VSSM.Application.DTOs.BaseEntities.Arrival;

public class ArrivalCreateDto
{
    public DateTime Date { get; set; }
    public string ReportNumber { get; set; } = "";
    public int FiscalPeriodId { get; set; }
    public int EnterById { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsDrafted { get; set; }
    public bool IsCancelled { get; set; }
    public string Note { get; set; } = "";  
    public string ArrivalType { get; set; } = "vaccine";


    public List<ArrivalLineDto> Lines { get; set; } = new();
    public int UserRecipientId { get; set; }
}

public class ArrivalEditDto : ArrivalCreateDto
{
    public int ArrivalId { get; set; }
}
