
namespace VSSM.Application.DTOs.BaseEntities.OrderInfo;
public record OrderInfoDto
{
    public int Id { get; internal set; }
    public string FileName { get; internal set; }
    public string CreationTime { get; internal set; }
    public string CreatedBy { get; internal set; }
    public int Quantity { get; internal set; }
    public string BatchNumber { get; internal set; }
    public int Status { get; internal set; }
}
