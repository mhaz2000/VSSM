namespace VSSM.Application.DTOs.RequestEntities;

public class RequestLineDto
{
    public int Id { get; set; }
    public int? RequestId { get; set; }

    public int? ItemId { get; set; }
    public string ItemName { get; set; } = "";

    public int? ItemClassId { get; set; }

    public int? Arivcount { get; set; }

    public int? Usedcount { get; set; }

    public int? Currentcount { get; set; }

    public int? Reqcount { get; set; }

    public string Note { get; set; } = "";

    public int? Confirmcount { get; set; }

    public int? DosePerVialId { get; set; }
}