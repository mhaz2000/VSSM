using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Store;

public class StoreViewModel
{
    public int? StoreId { get; set; }

    [Required(ErrorMessage = "کد نمی‌تواند خالی باشد.")]
    public string Code { get; set; }

    [Required(ErrorMessage = "نام انبار نمی‌تواند خالی باشد.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "نوع درجه حرارت نمی‌تواند خالی باشد.")]
    public int? TypeId { get; set; }

    [Required(ErrorMessage = "حجم نمی‌تواند خالی باشد.")]
    public float? FreeVolume { get; set; }

    public float? Volume { get; set; }

    public string? Note { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "گیرنده نمی‌تواند خالی باشد.")]
    public int? RecipientId { get; set; }

    public int? Year { get; set; }
}