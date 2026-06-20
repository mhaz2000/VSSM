using VSSM.Domain.Enums;
using VSSM.Application.Helpers;

namespace VSSM.Application.DTOs.RequestEntities;

public class RequestListDto
{
    public int Id { get; set; }
    public int? EnteredBy { get; set; } = null;
    public string EnteredByName { get; set; } = "";
    public DateOnly? SendDate { get; set; }
    public string RequestNumber { get; set; } = "";
    public DateOnly? ConfirmDate { get; set; }
    public int? RequestStatus { get; set; }
    public RequestStatusEnum Status { get; set; }

    public string SendDateShamsi => SendDate?.ToShamsi() ?? "-";
    public string ConfirmDateShamsi => ConfirmDate?.ToShamsi() ?? "-";
}
