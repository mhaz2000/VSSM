namespace VSSM.Presentation.Web.Models.Request;

public class RequestLinesViewModel
{
    public int Id { get; set; }

    public int? RequestId { get; set; }

    public int? ItemId { get; set; }
    public string ItemName { get; set; } = "";

    public int? ItemClassId { get; set; }

    public int? ArivCount { get; set; }

    public int? UsedCount { get; set; }

    public int? CurrentCount { get; set; }

    public int? ReqCount { get; set; }

    public string Note { get; set; } = "";

    public int? ConfirmCount { get; set; }

    public int? DosePerVialId { get; set; }

    // این دوتا توی سند نبودن (نه توی tblRequest نه tblRequestLine) — فعلاً نگه‌داشتم چون توی فرم استفاده می‌شن
    // اگه واقعاً بی‌مصرفن بگو حذفشون کنم
    public int? InventoryHealth { get; set; }
    public int? TimeBetweenRequestAndArrival { get; set; }
}