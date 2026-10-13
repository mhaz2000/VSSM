
using System.ComponentModel.DataAnnotations;

public class RequestLinesViewModel
{
    public int Id { get; set; }

    public int? RequestId { get; set; }

    [Required(ErrorMessage = "انتخاب کالا الزامی است.")]
    [Range(1, int.MaxValue, ErrorMessage = "کالای انتخاب‌شده معتبر نیست.")]
    public int? ItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب نوع کالا الزامی است.")]
    [Range(1, int.MaxValue, ErrorMessage = "نوع کالای انتخاب‌شده معتبر نیست.")]
    public int? ItemClassId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "تعداد ورودی نمی‌تواند منفی باشد.")]
    public int? ArivCount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "تعداد مصرف‌شده نمی‌تواند منفی باشد.")]
    public int? UsedCount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "موجودی فعلی نمی‌تواند منفی باشد.")]
    public int? CurrentCount { get; set; }

    [Required(ErrorMessage = "تعداد درخواستی الزامی است.")]
    [Range(1, int.MaxValue, ErrorMessage = "تعداد درخواستی باید بیشتر از صفر باشد.")]
    public int? ReqCount { get; set; }

    [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد.")]
    public string Note { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "تعداد تأییدشده نمی‌تواند منفی باشد.")]
    public int? ConfirmCount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "دوز انتخاب‌شده معتبر نیست.")]
    public int? DosePerVialId { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "مقدار موجودی سلامت نمی‌تواند منفی باشد.")]
    public int? InventoryHealth { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "فاصله زمانی نمی‌تواند منفی باشد.")]
    public int? TimeBetweenRequestAndArrival { get; set; }
}
