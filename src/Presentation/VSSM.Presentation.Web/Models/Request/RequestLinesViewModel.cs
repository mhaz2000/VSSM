namespace VSSM.Presentation.Web.Models.Request;

public class RequestLinesViewModel
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

    // این دوتا توی سند نبودن (نه توی tblRequest نه tblRequestLine) — فعلاً نگه‌داشتم چون توی فرم استفاده می‌شن
    // اگه واقعاً بی‌مصرفن بگو حذفشون کنم
    public int? InventoryHealth { get; set; }
    public int? TimeBetweenRequestAndArrival { get; set; }
}