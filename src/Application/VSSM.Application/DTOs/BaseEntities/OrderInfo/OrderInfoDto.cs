
namespace VSSM.Application.DTOs.BaseEntities.OrderInfo;

public record OrderInfoDto
{
    public int Id { get; internal set; }
    public string FileName { get; internal set; }
    public string CreationTime { get; internal set; }
    public string CreatedBy { get; internal set; }
    public int Quantity { get; internal set; }
    public string BatchNumber { get; internal set; }
    public string Status { get; internal set; }


    public DateTime? ExpirationDate { get; internal set; }
    public DateTime? ManufactureDate { get; internal set; }
    public bool IsConfirmed { get; internal set; }


    public int FkItemId { get; internal set; }
    public int FkDosePerVialId { get; internal set; }
    public string ItemName { get; internal set; } = "";
    public string DoseName { get; internal set; } = "";
}
