using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Store;

public class StoreViewModel
{
    public int? StoreId { get; set; }

    [Required(ErrorMessage = "کد الزامی است")]
    [StringLength(50, ErrorMessage = "کد حداکثر 50 کاراکتر می‌تواند باشد")]
    [Display(Name = "کد")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "نام انبار الزامی است")]
    [StringLength(200, ErrorMessage = "نام انبار حداکثر 200 کاراکتر می‌تواند باشد")]
    [Display(Name = "نام انبار")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "نوع انبار الزامی است")]
    [Display(Name = "نوع انبار")]
    public int TypeId { get; set; }

    [Required(ErrorMessage = "مالک الزامی است")]
    [Display(Name = "مالک")]
    public int? RecipientId { get; set; }

    [Display(Name = "حجم کلی فضا (لیتر)")]
    [Range(0, float.MaxValue, ErrorMessage = "حجم کلی باید عددی مثبت باشد")]
    public float? Volume { get; set; } 

    [Display(Name = "حجم مفید")]
    [Range(0, float.MaxValue, ErrorMessage = "حجم مفید باید عددی مثبت باشد")]
    public float? FreeVolume { get; set; }

    [Display(Name = "سال نصب")]
    public float? Year { get; set; }  

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "ملاحظات")]
    [StringLength(500, ErrorMessage = "ملاحظات حداکثر 500 کاراکتر می‌تواند باشد")]
    public string? Note { get; set; }

}