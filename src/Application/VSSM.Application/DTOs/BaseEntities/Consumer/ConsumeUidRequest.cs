namespace VSSM.Application.DTOs.BaseEntities.Consumer;

public class ConsumeUidRequest
{
    public string SiyamCode { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public string Vaccinator { get; set; } = string.Empty;
    public string ConsumerPerson { get; set; } = string.Empty;
}